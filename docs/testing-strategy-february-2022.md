# TMS testing strategy (as of February 2022)

After a change to TMS or the panellist portal, the following testing takes place:

1. Unit tests
2. User testing of specific changes
3. Regression testing
4. Smoke testing in live

## Unit tests

These need to be executed before the changes are applied in pre-prod.

|     |     |
| --- | --- |
| **Entry criteria** | Code changes have been completed |
| **Environment** | Dev |
| **Performed by** | CRM developer |
| **Exit criteria** | all unit tests have passed |

## User testing of specific changes

|     |     |
| --- | --- |
| **Entry criteria** | All unit tests have passed and the changes have been successfully deployed to pre-prod |
| **Environment** | Pre-prod |
| **Performed by** | TMU systems team |
| **Exit criteria** | No outstanding major bugs |

## Regression testing

The regression packs can be found [here](https://drive.google.com/drive/folders/1bmksIVRVP7ywbeK27_0ah2rZAkM3rlSB).

|     |     |
| --- | --- |
| **Entry criteria** | The tests can be run multiple times but the final run needs to occur after the specific changes have been tested and there are no outstanding major bugs |
| **Environment** | Pre-prod |
| **Performed by** | TMU systems team |
| **Exit criteria** | No outstanding major bugs |

## Smoke testing in Live

Once released to live, TMU staff make sure that the changes have been deployed.
There is no other testing done in the live environment.
