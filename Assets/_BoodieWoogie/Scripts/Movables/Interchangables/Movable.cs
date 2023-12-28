using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movable : MonoBehaviour, IInterchangable
{
    [SerializeField]
    private GameObject SelectedSpriteR;
    [SerializeField]
    private GameObject SelectedSpriteL;
    private bool isSelected = false;

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public void Interchange(Vector2 position)
    {
        transform.position = position;
    }

    public bool IsSelected()
    {
        return isSelected;
    }

    public void Select(EInterchangableSide side)
    {
        isSelected = true;
        if (!SelectedSpriteR || !SelectedSpriteL)
        {
            return;
        }
        if (side == EInterchangableSide.R)
        {
            SelectedSpriteR.SetActive(true);
            return;
        }
        SelectedSpriteL.SetActive(true);
    }

    public void Unselect()
    {
        isSelected = false;
        if (!SelectedSpriteR || !SelectedSpriteL)
        {
            return;
        }
        SelectedSpriteR.SetActive(false);
        SelectedSpriteL.SetActive(false);
    }
}
