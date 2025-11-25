using UnityEngine;

public class TutorialUISystem : MonoBehaviour
{
    // will need to prompt 'slideshow' of sorts, UI appears around necessary items in tutorial
    // have text that displays current tutorial step text
    // make sure only the boss on tutorial 3 drops credits, drops 40 to afford first core upgrade
    // will need to clear stats and upgrades when finished (going from TutorialLevel state -> Gameplay state)

    // ---------TUTORIAL OUTLINE ---------
    // *when first loading in, stop and point to enemies
    // STEP 1
    // turn based combat - "when you come across an enemy, you will attak each other automatically"
    // "kill all the enemies in a floor to progress to the next one!"
    // * after completing the first floor, stop before combat again
    // STEP 2
    // skill system - "these are your skills, click them or press 1,2,3 to use!"
    // "once you use a skill, it will need time to cool down before you can use it again"
    // "make sure you use these skills effectively!"
    // * after killing boss on 3rd floor, stop time to explain credits/upgrades
    // STEP 3
    // upgrades - "enemies drop credits when they die, these can be used to improve your stats!"
    // * go to upgrade screen and stop again
    // "this is the upgrade screen, each body part has levels to upgrade with scrap"
    // "go ahead and use your scrap to buy a CORE upgrade!"
    // *exit upgrade screen, stop one more time
    // FINAL
    // "Great! You are on your way to conquering the tower. Good luck!"
    // * have "FINISH" button to go to gameplay levels, clear all stats


    
}
