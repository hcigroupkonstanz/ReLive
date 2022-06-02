<h1 align="center">
  ReLive
</h1>

<p align="center">
    <strong>
      <a href="https://dl.acm.org/doi/10.1145/3491102.3517550">Publication</a>
        •
      <a href="https://youtu.be/BaNZ02QkZ_k">Prototype Video</a>
        •
      <a href="https://youtu.be/As3i9rzliF4">Presentation</a>
    </strong>
</p>

![The ReLive mixed-immersion tool. The tool combines an immersive analytics virtual reality view (left) with a synchronized non-immersive visual analytics desktop view (right) for analyzing mixed reality user studies. The virtual reality view allows users to relive and analyze the original study in-situ, while the desktop view facilitates an ex-situ analysis of aggregated study data.](/figures/relive.jpg?raw=true) 

This is the code repository of the CHI'22 publication:

> Sebastian Hubenschmid*, Jonathan Wieland*, Daniel Fink*, Andrea Batch, Johannes Zagermann, Niklas Elmqvist, and Harald Reiterer. 2022. ReLive: Bridging In-Situ and Ex-Situ Visual Analytics for Analyzing Mixed Reality User Studies. In *CHI Conference on Human Factors in Computing Systems (CHI’22)*, Apr 30–May 5, 2022, New Orleans, US. ACM, New York, NY, USA, 21 pages. doi: [10.1145/3491102.3517550](https://doi.org/10.1145/3491102.3517550) <sup>*The first three authors contributed equally</sup>

For questions or feedback, please contact [Sebastian Hubenschmid](https://hci.uni-konstanz.de/personen/wissenschaftliche-mitarbeiterinnen/sebastian-hubenschmid/) ([GitHub](https://github.com/SebiH)), [Jonathan Wieland](https://hci.uni-konstanz.de/personen/wissenschaftliche-mitarbeiterinnen/jonathan-wieland/) ([GitHub](https://github.com/WielandJ)), or [Daniel Fink](https://hci.uni-konstanz.de/personen/wissenschaftliche-mitarbeiterinnen/daniel-fink/) ([GitHub](https://github.com/dunifi91)).

The repository contains code for the following applications:
- **relive-desktop**: ReLive Desktop application for viewing MR studies on the desktop.
- **relive-logging-toolkit**: Versatile Unity logging toolkit for capturing data from mixed reality studies.
- **relive-server**: ReLive server that collects data from the toolkit and provides/synchronizes data between the ReLive-Desktop and ReLive-VR application.
- **relive-vr**: ReLive Unity application for viewing MR studies in VR.

For documentation, please see the individual project folders.
