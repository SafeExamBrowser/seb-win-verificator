# SEB Verificator

A standalone tool to manually verify the integrity of a Safe Exam Browser installation on Windows. This application is intended for use cases like BYOD exams where candidates have complete control over their
systems and thus could be trying to use a manipulated build of Safe Exam Browser to perform an exam. You may use the SEB Verificator on a USB flash drive and start it from there under your supervision.
Alternatively, you could make the tool downloadable on a website under your control and ask the candidate to download and execute the SEB Verificator under your supervision.

## Requirements

The application requires the prerequisites listed below in order to work correctly. These should already be present on a system which has SEB installed.

* .NET Framework 4.7.2 Runtime: https://dotnet.microsoft.com/download/dotnet-framework/net472

## Usage

Download and unzip the archive containing the [latest release version](https://github.com/SafeExamBrowser/seb-win-verificator/releases), then start the application by double-clicking `Verificator.exe`.
The tool will automatically search for a Safe Exam Browser installation on the system and display the version information and installation path under "Local Installation" once an installation has been found.

By default, the application comes with the references for all the official release versions since SEB 3.2.0 preloaded. Once there is at least one valid reference loaded and a local installation found,
the verification of the local installation can be started. After the verification procedure has finished, the status of all elements in the local installation will be displayed in the user interface.

The application can automatically start the local SEB installation once the verification procedure has finished successfully. It will furthermore search the folder containing the main executable
`Verificator.exe` as well as its parent directory for a SEB configuration file to use. When choosing to start SEB automatically, the application will terminate itself after SEB has been started.

## Project Status

**_DISCLAIMER_**\
**The builds linked below are for testing purposes only.** They may be unstable and should thus _never_ be used in a production environment! Always use the latest, official release version.

| Aspect            | Status                                                                                                                | Details                                                         |
| ----------------- | --------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------- |
| Development Build | ![Development Build Status](https://sebdev-let.ethz.ch/api/projects/status/pptmm2tt43scnj5w?svg=true)                 | https://sebdev-let.ethz.ch/project/appveyor/seb-win-verificator |
| Issue Status      | ![GitHub Issues](https://img.shields.io/github/issues/safeexambrowser/seb-win-verificator?logo=github)                | https://github.com/SafeExamBrowser/seb-win-verificator/issues   |
| Downloads         | ![GitHub All Releases](https://img.shields.io/github/downloads/safeexambrowser/seb-win-verificator/total?logo=github) | https://github.com/SafeExamBrowser/seb-win-verificator/releases |
| Development       | ![GitHub Last Commit](https://img.shields.io/github/last-commit/safeexambrowser/seb-win-verificator?logo=github)      | n/a                                                             |
| Repository Size   | ![GitHub Repo Size](https://img.shields.io/github/repo-size/safeexambrowser/seb-win-verificator?logo=github)          | n/a                                                             |
