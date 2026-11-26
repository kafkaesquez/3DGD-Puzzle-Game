using UnityEngine;
using UnityEngine.UI;

public class StarDisplay : MonoBehaviour
{
    [SerializeField] Image[] stars = new Image[3];
    [SerializeField] Sprite filledStar;
    [SerializeField] Sprite emptyStar;
    void OnEnable()
    {
        bool[] earned =
        {
            true, // checks the bool for all the items in an array (thats why , is used)
            LevelStarData.BeatTime, // beat the target time
            LevelStarData.GoldCollected // grabbed the gold collectible
        };

        for (int i = 0; i < stars.Length; i++)
            stars[i].sprite = earned[i] ? filledStar : emptyStar; //replace the sprite with the "filled" star
    }
}
