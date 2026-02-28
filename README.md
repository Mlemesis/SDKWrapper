# SDKWrapper
This pacakges wraps an UnstableSDK to allow games to have more control over the glitches and errors.

## Useage
Unity version built in 2022.3.62f2
ResiliantAnanlyticsSystem provides the wrapper and functions to help control when events are to be sent 
<img width="927" height="814" alt="image" src="https://github.com/user-attachments/assets/95c79536-2a79-4eb4-b5ef-524c4bff4008" />

An editor tool can be accessed from CT Tools/Resilient Analytics Monitor, this shows all events sent and indicates system stability.
<img width="1028" height="914" alt="image" src="https://github.com/user-attachments/assets/3667d4fe-072b-420a-8c00-579981604405" />


Provided is a SystemBootrapper which setup the various systems used. This is on an object in SampleScene and the system can be run from there. This also contains some debug keys for sending/blocking/unblocking events 
<img width="892" height="541" alt="image" src="https://github.com/user-attachments/assets/9b4fc98e-2e49-4052-a6cc-c9547823bf76" />


## Additional
SystemsBase and EventSystem has also been added to the project. These are useful base systems that are good for any project. Ordinarily these would be there own packages and included using the package manager.

ResiliantAnalyticsV2 this is a less developed version of the system, which overrides the Unity Random to allow us to use async. This has been left in as a conversation point but not as the main solution.

## AI usage
Claude is built into Rider and this was used to help generate the code for the editor. Editor code can be difficult to get right and using AI to generate it means you can quickly try different layouts and functionality.
Claude was also used ot help generate the Unit tests, using a third party gives you better coverage and allows for more indepth testing that you might think off yourself. 
