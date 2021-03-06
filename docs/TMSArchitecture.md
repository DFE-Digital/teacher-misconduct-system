# Teacher Misconduct System Architecture

## Overview

The Teacher Misconduct System is based on Microsoft Dynamics 365, with a document generation VM hosted in Tier 1 Azure.  Documents generated with this system are stored in the main DfE Sharepoint deployment.

There are extensive customisations deployed into the CRM system as CRM Solutions, and these include the entities, processes and workflows that make up the system.

## Production Architecture Diagram

![TMS Architecture PROD](images/structurizr-1-TMUTMS.png)

## CRM Solutions

### How the TMS CRM has been customised
The TMS system is implemented as an extension of the baseline CRM entities that are available off the shelf.  The customisations are implemented over a set of 12 solutions - each of them contribute changes to entities, processes and workflows.  Many solutions update the same entities, and the install order of the solutions is important.

### The significance of Managed vs Unmanaged solutions
All of the customisations for the CRM to meet TMU needs have been done via _unmanaged_ solutions, including the production environment.  This means that the components included inside each solution are applied already, and even if the solution is deleted, the changes the solution contained still remain.  This is as opposed to managed solutions, which have clear dependency paths, and can be completely uninstalled along with the components contained within them.

Microsoft Best Practices encourage the use of _unmananged_ solutions in development environments, and as assets in source control, but stipualte that managed solutions should be used in production environments where possible.

For more information, please see [Solution Concepts](https://docs.microsoft.com/en-us/power-platform/alm/solution-concepts-alm)

## Panellist Poral
The Panelist Portal is a web application that TMU Panellists log in to in order to conduct their role.  This portal is a CRM hosted portal, and is accessed via the public internet.

## Document Storage
Documents relating to cases in the TMS are stored in sharepoint.  The main DfE sharepoint deployment contains a site that holds all documents relating to the production environment of the TMS.

## Document Core Pack Server
This is a Windows 2016 Virtual Machine that is supported internally at the DfE.  This server holds the document templates that are used in the document generation process.

## Power Platform - Cloud Flows
There is a single cloud flow that operates on TMS data, and that flow appears to update a date column in each case record.

## Other environments

There are three software environments for the TMS system:
* PROD
* PRE-PROD
* DEV

The PROD environment has been documented as above, but the PRE-PROD and DEV environments do not have parity with the PROD environment.
Both PRE-PROD and DEV do not have a Document Pack Server - there is only one instance of this server across all of the environments.
Also, the number of solutions and their status differs between environments.

