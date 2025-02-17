using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utilities : MonoBehaviour
{
    public static List<Vector3Int> GetValidTiles(BaseUnit unit, string rangeType, int range = 1)
    {
        Vector3Int currPosition = unit.CurrentPosition;
        List<Vector3Int> tiles = new List<Vector3Int>();

        if (rangeType == "orthogonal")
        {
            for (int i = 1; i <= range; i++)
            {
                tiles.Add(currPosition + Vector3Int.up * i);
                tiles.Add(currPosition + Vector3Int.down * i);
                tiles.Add(currPosition + Vector3Int.left * i);
                tiles.Add(currPosition + Vector3Int.right * i);
            }
        }
        else if (rangeType == "square")
        {
            for (int x = -range; x <= range; x++)
            {
                for (int y = -range; y <= range; y++)
                {
                    tiles.Add(currPosition + new Vector3Int(x, y, 0));
                }
            }
        }

        return tiles;
    }

    public static BaseUnit GetUnitByViewID(int viewID) => PhotonView.Find(viewID).gameObject.GetComponent<BaseUnit>();

    public static void PlaySound(string play, float volume = 1f)
    {
        AudioSource audioSource = GameManager.Instance.audioSource;
        AudioClip soundClip = Resources.Load<AudioClip>("SoundEffects/" + play);

        if (soundClip == null)
        {
            Debug.LogWarning("Sound clip not found: " + play);
            return;
        }

        audioSource.clip = soundClip;
        audioSource.volume = volume;
        audioSource.Play();
    }
}
