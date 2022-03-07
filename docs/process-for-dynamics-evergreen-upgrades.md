# Process for Dynamics CRM evergreen upgrades

The updates typically happen in April and October and provide a number of updates. These updates are meant to improve the system and typically should not break any features. However, testing is required to ensure that the changes have no negative impact.

The schedule of updates can be found here (Europe region, not GB):

https://docs.microsoft.com/en-us/power-platform/admin/general-availability-deployment#deployment-schedule

The following steps should be followed:

1. The service delivery manager (SDM) receives messages about the updates from Office 365 message centre.
2. SDM makes the Teacher Misconduct Unit (TMU) aware and starts planning for the deployment and testing, involving TMU and TRA Digital CRM engineer.
3. Pre-prod is cloned in Azure and updates are applied. This is to ensure that pre-prod is not blocked for any other deployments that may be required.
4. TMU tests on the cloned environment.
5. If there are issues, the CRM engineer fixes the issues.
6. SDM and TMU agree a date on which to apply the updates to pre-prod and live.
7. SDM deletes the cloned pre-prod environment.
