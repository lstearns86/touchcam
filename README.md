# HandSight On-Body Interaction
This is the code repository for the HandSight On-Body Interaction Project, from the University of Maryland's Human Computer Interaction Lab. *(TODO: add a link to our related papers once they're publicly available)*

# Requirements:
## Hardware
* [NanEye GS Idule Module](http://www.cmosis.com/products/product_detail/naneyegs_idule_demo_kit)
* Custom sensor package using an Arduino microcontroller to transmit readings from two 9dof IMUs and two IR reflectance sensors. (*TODO: add schematics*)

## Software
* [Emgu v3.1 x64 CUDA](https://sourceforge.net/projects/emgucv/files/emgucv/3.1.0/libemgucv-windesktop_x64-cuda-3.1.0.2282.zip.selfextract.exe/download)
  * Should be installed in `C:\Emgu`, so that the libraries are located in `C:\Emgu\emgucv-windesktop_x64-cuda 3.1.0.2282\bin`. If you install it somewhere else, you'll need to modify the references and post-build settings to pull the libraries from the correct place.
* Other libraries are smaller and so will be included in this package. Here are their links for reference:
  * [NanEye GS Idule C# API v1.0.0](http://www.cmosis.com/?ACT=52&key=L0RUcWZFY0V2eml1VzVXZWZLNGUxb2hSR2M5cWY4V2IwUzVtTE5yeXZxOVZLdXlDY21GKzVGTnVncGQ3NGRCMGZBZ2dkQ1JLeWRlZFpKeWZLaXhnRVE9PQ==)
  * [AForge.NET v2.2.5](http://aforge.googlecode.com/files/AForge.NET%20Framework-2.2.5.exe)
  * [Accord.NET v3.0.2](https://github.com/accord-net/framework/releases/download/v3.0.0/Accord.NET-3.0.2-installer.exe)
  * [Alea GPU](http://quantalea.com/download/) (installed via NuGet)
  * [Json.NET](http://www.newtonsoft.com/json) (installed via NuGet)
