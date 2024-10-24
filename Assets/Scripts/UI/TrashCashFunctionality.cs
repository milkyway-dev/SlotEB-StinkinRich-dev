using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TrashCashFunctionality : MonoBehaviour
{
    [SerializeField]
    private BonusController _bonusManager;
    [SerializeField]
    private Button trash_Button;
    [SerializeField]
    private GameObject trashOpen_Object;
    [SerializeField]
    private GameObject trashClose_Object;
    [SerializeField]
    private TMP_Text Multiplier_Text;
    [SerializeField]
    private TMP_Text Value_Text;
    [SerializeField]
    private RectTransform Multiplier_Transform;
    [SerializeField]
    private RectTransform Value_Transform;
    [SerializeField]
    private GameObject Line_Object;

    private void Start()
    {
        
    }

    internal void OpenTrash()
    {
        if (trashOpen_Object) trashOpen_Object.SetActive(true);
        if (trashClose_Object) trashClose_Object.SetActive(false);
    }

    internal void ResetTrash()
    {

    }
}
