using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Emgu.CV;

namespace HandSightLibrary
{
    // Adapted from https://code.google.com/archive/p/9dof-orientation-estimation/
    public class KalmanFilter : Filter
    {
        private double sigmaRoll = Math.Pow((0.5647 / 180 * Math.PI), 2);           //the variance of the roll measure
        private double sigmaPitch = Math.Pow((0.5674 / 180 * Math.PI), 2);          //the variance of the pitch measure
        private double sigmaYaw = Math.Pow((0.5394 / 180 * Math.PI), 2);            //the variance of the yaw measure

        public double q_filt1, q_filt2, q_filt3, q_filt4;       //filtered quaternions

        Matrix<double> weOld;

        public List<double> aAcc;
        public List<double> bAcc;
        public List<double> aMagn;
        public List<double> bMagn;

        public List<double> AccObservX;
        public List<double> AccObservY;
        public List<double> AccObservZ;

        public List<double> AccFiltX;
        public List<double> AccFiltY;
        public List<double> AccFiltZ;

        public List<double> MagnObservX;
        public List<double> MagnObservY;
        public List<double> MagnObservZ;

        public List<double> MagnFiltX;
        public List<double> MagnFiltY;
        public List<double> MagnFiltZ;


        //int dim = 4;
        double w_x_old = 0;
        double w_y_old = 0;
        double w_z_old = 0;


        Matrix<double> F = new Matrix<double>(4, 4);
        Matrix<double> H = new Matrix<double>(new double[,] { { 1, 0, 0, 0 }, { 0, 1, 0, 0 }, { 0, 0, 1, 0 }, { 0, 0, 0, 1 } });
        Matrix<double> Q = new Matrix<double>(4, 4);
        Matrix<double> R = new Matrix<double>(new double[,] { { 1, 0, 0, 0 }, { 0, 1, 0, 0 }, { 0, 0, 1, 0 }, { 0, 0, 0, 1 } });


        Matrix<double> state_filtered = new Matrix<double>(4, 1);           //output of the filter. This is the update of the state
        Matrix<double> state_predicted = new Matrix<double>(4, 1);          //the predictiion of the state x(k|k-1), calculated from the gyro
        Matrix<double> state_observed = new Matrix<double>(4, 1);           //the observation calculated from the accelerometer and the magnetometer y(k). 

        Matrix<double> P_predicted = new Matrix<double>(4, 4);             //the variance of prediction of the state
        Matrix<double> P_Update = new Matrix<double>(4, 4);                 //the variance of the update of the state
        Matrix<double> K = new Matrix<double>(4, 4);                        //the gain of the filter

        Stack<double[]> MagnOsserv = new Stack<double[]>(4);
        Stack<double[]> AccOsserv = new Stack<double[]>(4);

        int countdata = 0;
        IIRFilter magnFilter;
        IIRFilter accFilter;

        public KalmanFilter()
        { 

            //Compute matrix Q (costant if the time interval is costant)
            Q[0, 0] = sigmaRoll + sigmaPitch + sigmaYaw;
            Q[0, 1] = -sigmaRoll + sigmaPitch - sigmaYaw;
            Q[0, 2] = -sigmaRoll - sigmaPitch + sigmaYaw;
            Q[0, 3] = sigmaRoll - sigmaPitch - sigmaYaw;
            Q[1, 0] = -sigmaRoll + sigmaPitch - sigmaYaw;
            Q[1, 1] = sigmaRoll + sigmaPitch + sigmaYaw;
            Q[1, 2] = sigmaRoll - sigmaPitch - sigmaYaw;
            Q[1, 3] = -sigmaRoll - sigmaPitch + sigmaYaw;
            Q[2, 0] = -sigmaRoll - sigmaPitch + sigmaYaw;
            Q[2, 1] = sigmaRoll - sigmaPitch - sigmaYaw;
            Q[2, 2] = sigmaRoll + sigmaPitch + sigmaYaw;
            Q[2, 3] = -sigmaRoll + sigmaPitch - sigmaYaw;
            Q[3, 0] = sigmaRoll - sigmaPitch - sigmaYaw;
            Q[3, 1] = -sigmaRoll - sigmaPitch + sigmaYaw;
            Q[3, 2] = -sigmaRoll + sigmaPitch - sigmaYaw;
            Q[3, 3] = sigmaRoll + sigmaPitch + sigmaYaw;

            R = new Matrix<double>(4, 4);
            R[0, 0] = 0.2;
            R[0, 1] = 0;
            R[0, 2] = 0;
            R[0, 3] = 0;

            R[1, 0] = 0;
            R[1, 1] = 0.2;
            R[1, 2] = 0;
            R[1, 3] = 0;

            R[2, 0] = 0;
            R[2, 1] = 0;
            R[2, 2] = 0.2;
            R[2, 3] = 0;

            R[3, 0] = 0;
            R[3, 1] = 0;
            R[3, 2] = 0;
            R[3, 3] = 0.2;



            //Initial conditions
            state_observed = new MyQuaternion(0.5, 0.5, 0.5, 0.5).getQuaternionAsVector();
            state_filtered = new MyQuaternion(0.5, 0.5, 0.5, 0.5).getQuaternionAsVector();
            state_predicted = new MyQuaternion(0.5, 0.5, 0.5, 0.5).getQuaternionAsVector();
            P_Update = new Matrix<double>(new double[,] { { 1, 0, 0, 0 }, { 0, 1, 0, 0 }, { 0, 0, 1, 0 }, { 0, 0, 0, 1 } });
            P_Update = P_Update.Mul(0.1);

            aAcc = new List<double>();
            bAcc = new List<double>();
            aMagn = new List<double>();
            bMagn = new List<double>();

            aAcc.Add(1);
            aAcc.Add(-2.9529);
            aAcc.Add(2.9069);
            aAcc.Add(-0.954);

            bAcc.Add(0.000001597);
            bAcc.Add(0.000004792);
            bAcc.Add(0.000004792);
            bAcc.Add(0.000001597);

            accFilter = new IIRFilter(aAcc, bAcc);

            aMagn.Add(1);
            aMagn.Add(-1.73);
            aMagn.Add(0.76);

            bMagn.Add(0.0078);
            bMagn.Add(0.0156);
            bMagn.Add(0.0078);

            magnFilter = new IIRFilter(aMagn, bMagn);

            AccObservX = new List<double>();
            AccObservY = new List<double>();
            AccObservZ = new List<double>();
            AccFiltX = new List<double>();
            AccFiltY = new List<double>();
            AccFiltZ = new List<double>();

            MagnObservX = new List<double>();
            MagnObservY = new List<double>();
            MagnObservZ = new List<double>();
            MagnFiltX = new List<double>();
            MagnFiltY = new List<double>();
            MagnFiltZ = new List<double>();

            weOld = new Matrix<double>(4, 1);
            weOld = weOld.Mul(0);

            
        }

        public override void filterStep(double w_x, double w_y, double w_z, double a_x, double a_y, double a_z, double m_x, double m_y, double m_z)
        {
            double norm;
            Matrix<double> temp;//=new Matrix(4,4);
            Matrix<double> dq = new Matrix<double>(4, 1);
            double mu;
            double k;

            // normalise the accelerometer measurement
            norm = Math.Sqrt(a_x * a_x + a_y * a_y + a_z * a_z);
            a_x /= norm;
            a_y /= norm;
            a_z /= norm;

            // normalise the magnetometer measurement
            norm = Math.Sqrt(m_x * m_x + m_y * m_y + m_z * m_z);
            m_x /= norm;
            m_y /= norm;
            m_z /= norm;

            if (AccObservX.Count < 10)
            {
                AccObservX.Add(a_x);
                AccObservY.Add(a_y);
                AccObservZ.Add(a_z);
            }
            else
            {
                AccObservX.RemoveAt(0);
                AccObservY.RemoveAt(0);
                AccObservZ.RemoveAt(0);

                AccObservX.Add(a_x);
                AccObservY.Add(a_y);
                AccObservZ.Add(a_z);
            }
            if (MagnObservX.Count < 10)
            {
                MagnObservX.Add(m_x);
                MagnObservY.Add(m_y);
                MagnObservZ.Add(m_z);
            }
            else
            {
                MagnObservX.RemoveAt(0);
                MagnObservY.RemoveAt(0);
                MagnObservZ.RemoveAt(0);
                MagnObservX.Add(m_x);
                MagnObservY.Add(m_y);
                MagnObservZ.Add(m_z);

                //Filter stabilization
                accFilter.Applyfilter(AccObservX, out AccFiltX);
                accFilter.Applyfilter(AccObservY, out AccFiltY);
                accFilter.Applyfilter(AccObservZ, out AccFiltZ);

                magnFilter.Applyfilter(MagnObservX, out MagnFiltX);
                magnFilter.Applyfilter(MagnObservY, out MagnFiltY);
                magnFilter.Applyfilter(MagnObservZ, out MagnFiltZ);
            }

            if (countdata > 10)
            {
                a_x = AccFiltX.Last();
                a_y = AccFiltY.Last();
                a_z = AccFiltZ.Last();

                m_x = MagnFiltX.Last();
                m_y = MagnFiltY.Last();
                m_z = MagnFiltZ.Last();

                // normalise the accelerometer measurement
                norm = Math.Sqrt(a_x * a_x + a_y * a_y + a_z * a_z);
                a_x /= norm;
                a_y /= norm;
                a_z /= norm;


                // normalise the magnetometer measurement
                norm = Math.Sqrt(m_x * m_x + m_y * m_y + m_z * m_z);
                m_x /= norm;
                m_y /= norm;
                m_z /= norm;
            }



            countdata++;

            w_x = w_x - gyroOffRoll;
            w_y = -w_y + gyroOffPitch;
            w_z = w_z - gyroOffYaw;

            w_x = w_x * Math.PI / 180;
            w_y = w_y * Math.PI / 180;
            w_z = w_z * Math.PI / 180;


            if (this.obsMethod == 0)
            {
                dq = 0.5 * (MyQuaternion.quaternionProduct(state_observed, new MyQuaternion(0.0, w_x, w_y, w_z).getQuaternionAsVector()));
                norm = Math.Sqrt(dq[0, 0] * dq[0, 0] + dq[1, 0] * dq[1, 0] + dq[2, 0] * dq[2, 0] + dq[3, 0] * dq[3, 0]);
                mu = 10 * norm * dt;
                state_observed = GradientDescent(a_x, a_y, a_z, m_x, m_y, m_z, mu, state_filtered);
            }
            else
                state_observed= gaussNewtonMethod(a_x, a_y, a_z, m_x, m_y, m_z, state_filtered);

            //KALMAN FILTERING

            //F matrix computing
            F[0, 0] = 1;
            F[0, 1] = -dt / 2 * w_x;
            F[0, 2] = -dt / 2 * w_y;
            F[0, 3] = -dt / 2 * w_z;
            F[1, 0] = dt / 2 * w_x;
            F[1, 1] = 1;
            F[1, 2] = dt / 2 * w_z;
            F[1, 3] = -dt / 2 * w_y;
            F[2, 0] = dt / 2 * w_y;
            F[2, 1] = -dt / 2 * w_z;
            F[2, 2] = 1;
            F[2, 3] = dt / 2 * w_x;
            F[3, 0] = dt / 2 * w_z;
            F[3, 1] = dt / 2 * w_y;
            F[3, 2] = -dt / 2 * w_x;
            F[3, 3] = 1;

            //state prediction
            state_predicted = F * state_filtered;

            //calculate the variance of the prediction
            P_predicted = F * P_Update;
            P_predicted = P_predicted * F.Transpose() + Q;

            //compute the gain of the filter K
            temp = (H * P_predicted * H.Transpose()) + R;
            CvInvoke.Invert(temp, temp, Emgu.CV.CvEnum.DecompMethod.Svd);
            //temp = Matrix.Inverse(temp);
            K = ((P_predicted * H.Transpose()) * temp);

            //update the state of the system (this is the output of the filter)
            temp = (state_observed - (H * state_predicted));
            state_filtered = (state_predicted + (K * temp));

            //compute the variance of the state filtered
            temp = ((new Matrix<double>(new double[,] { { 1, 0, 0, 0 }, { 0, 1, 0, 0 }, { 0, 0, 1, 0 }, { 0, 0, 0, 1 } })) - (K * H));
            P_Update = (temp * P_predicted);

            norm = Math.Sqrt(state_filtered[0, 0] * state_filtered[0, 0] + state_filtered[1, 0] * state_filtered[1, 0] + state_filtered[2, 0] * state_filtered[2, 0] + state_filtered[3, 0] * state_filtered[3, 0]);
            state_filtered = state_filtered.Mul(1.0 / norm);// Matrix.ScalarDivide(norm, state_filtered);
            q_filt1 = state_filtered[0, 0];
            q_filt2 = state_filtered[1, 0];
            q_filt3 = state_filtered[2, 0];
            q_filt4 = state_filtered[3, 0];
        }

        public override Matrix<double> getFilteredQuaternions() {
            return this.state_filtered;
        }

        public override Matrix<double> getFilteredAngles() {
            return MyQuaternion.getAnglesFromQuaternion(state_filtered);
        }
    }
}
