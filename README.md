# SDKWrapper
This pacakges wraps an UnstableSDK to allow games to have more control over the glitches and errors.

##Useage
ResiliantAnanlyticsSystem provides the wrapper and functions to help control when events are to be sent 
<img width="927" height="814" alt="image" src="https://github.com/user-attachments/assets/95c79536-2a79-4eb4-b5ef-524c4bff4008" />

An editor tool can be accessed from CT Tools/Resilient Analytics Monitor, this shows all events sent and indicates system stability.
<img width="886" height="1015" alt="image" src="https://github.com/user-attachments/assets/4640c846-ccd7-4533-b760-a4055d8e1ec4" />

Provided is a SystemBootrapper which setup the various systems used. This is on an object in SampleScene and the system can be run from there. 

##Additional
SystemsBase and EventSystem has also been added to the project. These are useful base systems that are good for any project. Ordinarily these would be there own packages and included using the package manager.
ResiliantAnalyticsV2 this is a less developed version of the system, which overrides the Unity Random to allow us to use async. This has been left in as a conversation point but not as the main solution.
