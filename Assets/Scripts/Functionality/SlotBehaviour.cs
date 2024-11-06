using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using System;

public class SlotBehaviour : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField]
    private Sprite[] myImages;  //images taken initially
    [SerializeField]
    private Sprite[] KTRImages;  
    [SerializeField]
    private List<Sprite> Box_Sprites;

    [Header("Slot Images")]
    [SerializeField]
    private List<SlotImage> images;     //class to store total images
    [SerializeField]
    private List<SlotImage> KTRimages;     //class to store total images
    [SerializeField]
    private List<SlotImage> Tempimages;     //class to store the result matrix
    [SerializeField]
    private List<SlotImage> Animimages;     //class to store the animation matrix
    [SerializeField]
    private List<BoxScript> TempBoxScripts;

    [Header("Slots Transforms")]
    [SerializeField]
    private Transform[] Slot_Transform;
    [SerializeField]
    private Transform[] KTRSlot_Transform;

    private Dictionary<int, string> y_string = new Dictionary<int, string>();

    [Header("Buttons")]
    [SerializeField]
    private Button SlotStart_Button;
    [SerializeField]
    private Button KTRStart_Button;
    [SerializeField]
    private Button MaxBet_Button;
    [SerializeField]
    private Button BetPlus_Button;
    [SerializeField]
    private Button BetMinus_Button;
    [SerializeField]
    private Button TBPlus_Button;
    [SerializeField]
    private Button TBMinus_Button;
    [SerializeField]
    private Button Settings_Button;

    [Header("Animated Sprites")]
    [SerializeField]
    private Sprite[] Shoe_Sprite;
    [SerializeField]
    private Sprite[] Fish_Sprite;
    [SerializeField]
    private Sprite[] Cheese_Sprite;
    [SerializeField]
    private Sprite[] Garlic_Sprite;
    [SerializeField]
    private Sprite[] Egg_Sprite;
    [SerializeField]
    private Sprite[] Boy_Sprite;
    [SerializeField]
    private Sprite[] LadyCoat_Sprite;
    [SerializeField]
    private Sprite[] OldMan_Sprite;
    [SerializeField]
    private Sprite[] FatLady_Sprite;
    [SerializeField]
    private Sprite[] Wild_Sprite;
    [SerializeField]
    private Sprite[] Scatter_Sprite;
    [SerializeField]
    private Sprite[] Keys_Sprite;
    [SerializeField]
    private Sprite[] Trash_Sprite;

    [Header("KTR Animated Sprites")]
    [SerializeField]
    private Sprite[] ShoeKTR_Sprite;
    [SerializeField]
    private Sprite[] FishKTR_Sprite;
    [SerializeField]
    private Sprite[] CheeseKTR_Sprite;
    [SerializeField]
    private Sprite[] GarlicKTR_Sprite;
    [SerializeField]
    private Sprite[] EggKTR_Sprite;
    [SerializeField]
    private Sprite[] BoyKTR_Sprite;
    [SerializeField]
    private Sprite[] LadyCoatKTR_Sprite;
    [SerializeField]
    private Sprite[] OldManKTR_Sprite;
    [SerializeField]
    private Sprite[] FatLadyKTR_Sprite;
    [SerializeField]
    private Sprite[] WildKTR_Sprite;
    [SerializeField]
    private Sprite[] ScatterKTR_Sprite;
    [SerializeField]
    private Sprite[] KeysKTR_Sprite;
    [SerializeField]
    private Sprite[] TrashKTR_Sprite;

    [Header("Miscellaneous UI")]
    [SerializeField]
    private TMP_Text Balance_text;
    [SerializeField]
    private TMP_Text TotalBet_text;
    [SerializeField]
    private TMP_Text LineBet_text;
    [SerializeField]
    private TMP_Text TotalWin_text;


    [Header("Audio Management")]
    [SerializeField]
    private AudioController audioController;

    [SerializeField]
    private UIManager uiManager;

    [Header("BonusGame Popup")]
    [SerializeField]
    private BonusController _bonusManager;

    [SerializeField]
    int tweenHeight = 0;  //calculate the height at which tweening is done

    [SerializeField]
    private PayoutCalculation PayCalculator;

    private List<Tweener> alltweens = new List<Tweener>();

    private Tweener WinTween = null;

    [SerializeField]
    private List<ImageAnimation> TempList;  //stores the sprites whose animation is running at present 

    [SerializeField]
    private SocketIOManager SocketManager;

    private Coroutine BoxAnimRoutine = null;
    private Coroutine tweenroutine;

    internal bool IsFreeSpin = false;
    private bool IsSpinning = false;
    private bool CheckSpinAudio = false;
    internal bool CheckPopups = false;

    private int BetCounter = 0;
    private int FreeSpinCounter = 0;
    private double currentBalance = 0;
    private double currentTotalBet = 0;
    protected int Lines = 20;
    [SerializeField]
    private int IconSizeFactor = 100;       //set this parameter according to the size of the icon and spacing
    private int numberOfSlots = 5;          //number of columns


    private void Start()
    {

        if (SlotStart_Button) SlotStart_Button.onClick.RemoveAllListeners();
        if (SlotStart_Button) SlotStart_Button.onClick.AddListener(delegate { StartSlots(); });

        if (BetPlus_Button) BetPlus_Button.onClick.RemoveAllListeners();
        if (BetPlus_Button) BetPlus_Button.onClick.AddListener(delegate { ChangeBet(true); });
        if (BetMinus_Button) BetMinus_Button.onClick.RemoveAllListeners();
        if (BetMinus_Button) BetMinus_Button.onClick.AddListener(delegate { ChangeBet(false); });

        if (MaxBet_Button) MaxBet_Button.onClick.RemoveAllListeners();
        if (MaxBet_Button) MaxBet_Button.onClick.AddListener(MaxBet);

        if (TBPlus_Button) TBPlus_Button.onClick.RemoveAllListeners();
        if (TBPlus_Button) TBPlus_Button.onClick.AddListener(delegate { ToggleTotalBet(true); });

        if (TBMinus_Button) TBMinus_Button.onClick.RemoveAllListeners();
        if (TBMinus_Button) TBMinus_Button.onClick.AddListener(delegate { ToggleTotalBet(false); });

        if (KTRStart_Button) KTRStart_Button.onClick.RemoveAllListeners();
        if (KTRStart_Button) KTRStart_Button.onClick.AddListener(EnableFreespinPlayButton);
    }

    private void CompareBalance()
    {
        if (currentBalance < currentTotalBet)
        {
            uiManager.LowBalPopup();
            if (SlotStart_Button) SlotStart_Button.interactable = false;
        }
        else
        {
            if (SlotStart_Button) SlotStart_Button.interactable = true;
        }
    }

    #region LinesCalculation
    //Fetch Lines from backend
    internal void FetchLines(string LineVal, int count)
    {
        y_string.Add(count + 1, LineVal);
    }

    //Destroy Static Lines from button hovers
    internal void DestroyStaticLine()
    {
        PayCalculator.ResetStaticLine();
    }
    #endregion

    private void MaxBet()
    {
        if (audioController) audioController.PlayButtonAudio();
        BetCounter = SocketManager.initialData.Bets.Count - 1;
        if (LineBet_text) LineBet_text.text = SocketManager.initialData.Bets[BetCounter].ToString();
        if (TotalBet_text) TotalBet_text.text = (SocketManager.initialData.Bets[BetCounter] * Lines).ToString();
        currentTotalBet = SocketManager.initialData.Bets[BetCounter] * Lines;
        CompareBalance();
    }

    private void ChangeBet(bool IncDec)
    {
        if (audioController) audioController.PlayButtonAudio();
        if (IncDec)
        {
            if (BetCounter < SocketManager.initialData.Bets.Count - 1)
            {
                BetCounter++;
            }
            else
            {
                BetCounter = 0;
            }
        }
        else
        {
            if (BetCounter > 0)
            {
                BetCounter--;
            }
            else
            {
                BetCounter = SocketManager.initialData.Bets.Count - 1;
            }
        }
        if (LineBet_text) LineBet_text.text = SocketManager.initialData.Bets[BetCounter].ToString();
        if (TotalBet_text) TotalBet_text.text = (SocketManager.initialData.Bets[BetCounter] * Lines).ToString();
        currentTotalBet = SocketManager.initialData.Bets[BetCounter] * Lines;
        CompareBalance();
    }

    private void ToggleTotalBet(bool IncDec)
    {
        if (audioController) audioController.PlayButtonAudio();
        if (IncDec)
        {
            if (BetCounter < SocketManager.initialData.Bets.Count - 1)
            {
                BetCounter++;
            }
            else
            {
                BetCounter = 0;
            }
        }
        else
        {
            if (BetCounter > 0)
            {
                BetCounter--;
            }
            else
            {
                BetCounter = SocketManager.initialData.Bets.Count - 1;
            }
        }
        if (LineBet_text) LineBet_text.text = SocketManager.initialData.Bets[BetCounter].ToString();
        if (TotalBet_text) TotalBet_text.text = (SocketManager.initialData.Bets[BetCounter] * Lines).ToString("f2");
        currentTotalBet = SocketManager.initialData.Bets[BetCounter] * Lines;
        CompareBalance();
    }

    #region InitialFunctions
    internal void shuffleInitialMatrix()
    {
        for (int i = 0; i < Tempimages.Count; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                int randomIndex = UnityEngine.Random.Range(0, 11);
                Tempimages[i].slotImages[j].sprite = myImages[randomIndex];
            }
        }
    }

    private void shuffleInitialMatrixKTR()
    {
        for (int i = 0; i < KTRimages.Count; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                int randomIndex = UnityEngine.Random.Range(0, 11);
                KTRimages[i].slotImages[j].sprite = KTRImages[randomIndex];
            }
        }
    }

    internal void SetInitialUI()
    {
        BetCounter = 0;
        if (LineBet_text) LineBet_text.text = SocketManager.initialData.Bets[BetCounter].ToString();
        if (TotalBet_text) TotalBet_text.text = (SocketManager.initialData.Bets[BetCounter] * Lines).ToString();
        if (TotalWin_text) TotalWin_text.text = "0.00";
        if (Balance_text) Balance_text.text = SocketManager.playerdata.Balance.ToString("f2");
        currentBalance = SocketManager.playerdata.Balance;
        currentTotalBet = SocketManager.initialData.Bets[BetCounter] * Lines;
        CompareBalance();
        uiManager.InitialiseUIData(SocketManager.initUIData.paylines);
    }
    #endregion

    private void OnApplicationFocus(bool focus)
    {
        audioController.CheckFocusFunction(focus, CheckSpinAudio);
    }

    //function to populate animation sprites accordingly
    private void PopulateAnimationSprites(ImageAnimation animScript, int val)
    {
        animScript.textureArray.Clear();
        animScript.textureArray.TrimExcess();
        animScript.AnimationSpeed = 25f;
        switch (val)
        {
            case 0:
                for (int i = 0; i < Shoe_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Shoe_Sprite[i]);
                }
                break;
            case 1:
                for (int i = 0; i < Fish_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Fish_Sprite[i]);
                }
                break;
            case 2:
                for (int i = 0; i < Cheese_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Cheese_Sprite[i]);
                }
                break;
            case 3:
                for (int i = 0; i < Garlic_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Garlic_Sprite[i]);
                }
                break;
            case 4:
                for (int i = 0; i < Egg_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Egg_Sprite[i]);
                }
                break;
            case 5:
                for (int i = 0; i < Boy_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Boy_Sprite[i]);
                }
                animScript.AnimationSpeed = 80f;
                break;
            case 6:
                for (int i = 0; i < LadyCoat_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(LadyCoat_Sprite[i]);
                }
                animScript.AnimationSpeed = 80f;
                break;
            case 7:
                for (int i = 0; i < OldMan_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(OldMan_Sprite[i]);
                }
                animScript.AnimationSpeed = 140f;
                break;
            case 8:
                for (int i = 0; i < FatLady_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(FatLady_Sprite[i]);
                }
                animScript.AnimationSpeed = 30f;
                break;
            case 9:
                for (int i = 0; i < Wild_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Wild_Sprite[i]);
                }
                animScript.AnimationSpeed = 40f;
                break;
            case 10:
                for (int i = 0; i < Scatter_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Scatter_Sprite[i]);
                }
                animScript.AnimationSpeed = 35f;
                break;
            case 11:
                for (int i = 0; i < Keys_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Keys_Sprite[i]);
                }
                animScript.AnimationSpeed = 45f;
                break;
            case 12:
                for (int i = 0; i < Trash_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Trash_Sprite[i]);
                }
                animScript.AnimationSpeed = 10f;
                break;
        }
    }

    private void PopulateAnimationSpritesKTR(ImageAnimation animScript, int val)
    {
        animScript.textureArray.Clear();
        animScript.textureArray.TrimExcess();
        animScript.AnimationSpeed = 25f;
        switch (val)
        {
            case 0:
                for (int i = 0; i < ShoeKTR_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(ShoeKTR_Sprite[i]);
                }
                break;
            case 1:
                for (int i = 0; i < FishKTR_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(FishKTR_Sprite[i]);
                }
                break;
            case 2:
                for (int i = 0; i < CheeseKTR_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(CheeseKTR_Sprite[i]);
                }
                break;
            case 3:
                for (int i = 0; i < GarlicKTR_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(GarlicKTR_Sprite[i]);
                }
                break;
            case 4:
                for (int i = 0; i < EggKTR_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(EggKTR_Sprite[i]);
                }
                break;
            case 5:
                for (int i = 0; i < BoyKTR_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(BoyKTR_Sprite[i]);
                }
                animScript.AnimationSpeed = 20f;
                break;
            case 6:
                for (int i = 0; i < LadyCoatKTR_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(LadyCoatKTR_Sprite[i]);
                }
                animScript.AnimationSpeed = 20f;
                break;
            case 7:
                for (int i = 0; i < OldManKTR_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(OldManKTR_Sprite[i]);
                }
                animScript.AnimationSpeed = 30f;
                break;
            case 8:
                for (int i = 0; i < FatLadyKTR_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(FatLadyKTR_Sprite[i]);
                }
                animScript.AnimationSpeed = 30f;
                break;
            case 9:
                for (int i = 0; i < WildKTR_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(WildKTR_Sprite[i]);
                }
                animScript.AnimationSpeed = 95f;
                break;
            case 10:
                for (int i = 0; i < ScatterKTR_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(ScatterKTR_Sprite[i]);
                }
                animScript.AnimationSpeed = 90f;
                break;
            case 11:
                for (int i = 0; i < KeysKTR_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(KeysKTR_Sprite[i]);
                }
                animScript.AnimationSpeed = 45f;
                break;
            case 12:
                for (int i = 0; i < TrashKTR_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(TrashKTR_Sprite[i]);
                }
                animScript.AnimationSpeed = 10f;
                break;
        }
    }

    #region SlotSpin
    //starts the spin process
    private void StartSlots()
    {
        if (audioController) audioController.PlaySpinButtonAudio();

        WinningsAnim(false);
        if (SlotStart_Button) SlotStart_Button.interactable = false;
        if (TempList.Count > 0)
        {
            StopGameAnimation();
        }
        PayCalculator.ResetLines();
        tweenroutine = StartCoroutine(TweenRoutine());
    }

    //manage the Routine for spinning of the slots
    private IEnumerator TweenRoutine()
    {
        if (currentBalance < currentTotalBet && !IsFreeSpin) 
        {
            CompareBalance();
            yield return new WaitForSeconds(1);
            ToggleButtonGrp(true);
            yield break;
        }
        if(IsFreeSpin)
        {
            FreeSpinCounter--;
            uiManager.UpdateUI(FreeSpinCounter, 0);
        }
        if (uiManager) uiManager.ToggleSquirrel(true);
        if (audioController) audioController.PlayWLAudio("spin");
        CheckSpinAudio = true;

        IsSpinning = true;

        ToggleButtonGrp(false);

        for (int i = 0; i < numberOfSlots; i++)
        {
            if (!IsFreeSpin)
            {
                InitializeTweening(Slot_Transform[i]);
                yield return new WaitForSeconds(0.1f);
            }
            else
            {
                InitializeTweening(KTRSlot_Transform[i]);
                yield return new WaitForSeconds(0.1f);
            }
        }

        if (!IsFreeSpin)
        {
            BalanceDeduction();
        }
        SocketManager.AccumulateResult(BetCounter);

        yield return new WaitUntil(() => SocketManager.isResultdone);

        for (int j = 0; j < SocketManager.resultData.ResultReel.Count; j++)
        {
            List<int> resultnum = SocketManager.resultData.FinalResultReel[j]?.Split(',')?.Select(Int32.Parse)?.ToList();
            for (int i = 0; i < 5; i++)
            {
                if (!IsFreeSpin)
                {
                    if (images[i].slotImages[j]) images[i].slotImages[j].sprite = myImages[resultnum[i]];
                    if (Animimages[i].slotImages[j]) Animimages[i].slotImages[j].sprite = myImages[resultnum[i]];
                    PopulateAnimationSprites(Animimages[i].slotImages[j].gameObject.GetComponent<ImageAnimation>(), resultnum[i]);
                }
                else
                {
                    if (KTRimages[i].slotImages[j]) KTRimages[i].slotImages[j].sprite = KTRImages[resultnum[i]];
                    if (Animimages[i].slotImages[j]) Animimages[i].slotImages[j].sprite = KTRImages[resultnum[i]];
                    PopulateAnimationSpritesKTR(Animimages[i].slotImages[j].gameObject.GetComponent<ImageAnimation>(), resultnum[i]);
                }
            }
        }

        yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < numberOfSlots; i++)
        {
            if (!IsFreeSpin)
            {
                yield return StopTweening(5, Slot_Transform[i], i);
            }
            else
            {
                yield return StopTweening(5, KTRSlot_Transform[i], i);
            }

        }

        yield return new WaitForSeconds(0.3f);
        CheckPayoutLineBackend(SocketManager.resultData.linesToEmit, SocketManager.resultData.FinalsymbolsToEmit, SocketManager.tempBonus.trashForCashWinningSymbols, SocketManager.resultData.scatterWinningSymbols);
        KillAllTweens();

        CheckPopups = true;

        if (SocketManager.resultData.bonusData.isBonus)
        {
            CheckBonusGame();
        }
        else
        {
            CheckWinPopups();
        }

        yield return new WaitUntil(() => !CheckPopups);

        if (IsFreeSpin)
        {
            uiManager.UpdateUI(FreeSpinCounter, SocketManager.playerdata.currentWining);
        }
        if (Balance_text) Balance_text.text = SocketManager.playerdata.Balance.ToString("f2");

        currentBalance = SocketManager.playerdata.Balance;

        if (SocketManager.resultData.freeSpin.isNewAdded)
        {
            FreeSpinCounter = (int)SocketManager.resultData.freeSpin.freeSpinCount;
            if (IsFreeSpin)
            {
                ToggleButtonGrp(true);
                uiManager.FreeSpinProcessStart((int)SocketManager.resultData.freeSpin.freeSpinCount, currentTotalBet);
            }
            else
            {
                IsFreeSpin = true;
                shuffleInitialMatrixKTR();
                if (uiManager) uiManager.ToggleBonusRText(true);
                if (KTRStart_Button) KTRStart_Button.gameObject.SetActive(true);
            }
        }
        else
        {
            ToggleButtonGrp(true);
        }
        if (uiManager) uiManager.ToggleSquirrel(false);
        if (IsFreeSpin && FreeSpinCounter <= 0)
        {
            IsFreeSpin = false;
            uiManager.FreeSpinProcessStop();
            DOVirtual.DelayedCall(2f, () =>
            {
                ToggleButtonGrp(true);
            });
        }
        IsSpinning = false;
    }

    private void EnableFreespinPlayButton()
    {
        if (audioController) audioController.SwitchBGSound(true);
        if (KTRStart_Button) KTRStart_Button.gameObject.SetActive(false);
        if (uiManager) uiManager.ToggleBonusRText(false);
        uiManager.FreeSpinProcessStart((int)SocketManager.resultData.freeSpin.freeSpinCount, currentTotalBet, true);
        PayCalculator.ResetLines();
        StopGameAnimation();
        ToggleButtonGrp(true);
    }

    private void BalanceDeduction()
    {
        double bet = 0;
        double balance = 0;
        try
        {
            bet = double.Parse(TotalBet_text.text);
        }
        catch (Exception e)
        {
            Debug.Log("Error while conversion " + e.Message);
        }

        try
        {
            balance = double.Parse(Balance_text.text);
        }
        catch (Exception e)
        {
            Debug.Log("Error while conversion " + e.Message);
        }
        double initAmount = balance;

        balance = balance - bet;

        DOTween.To(() => initAmount, (val) => initAmount = val, balance, 0.8f).OnUpdate(() =>
        {
            if (Balance_text) Balance_text.text = initAmount.ToString("f2");
        });
    }

    internal void CheckWinPopups()
    {
        if (TotalWin_text) TotalWin_text.text = SocketManager.playerdata.currentWining.ToString();
        if (SocketManager.resultData.WinAmout >= currentTotalBet * 10 && SocketManager.resultData.WinAmout < currentTotalBet * 15)
        {
            uiManager.PopulateWin(1, SocketManager.resultData.WinAmout);
        }
        else if (SocketManager.resultData.WinAmout >= currentTotalBet * 15 && SocketManager.resultData.WinAmout < currentTotalBet * 20)
        {
            uiManager.PopulateWin(2, SocketManager.resultData.WinAmout);
        }
        else if (SocketManager.resultData.WinAmout >= currentTotalBet * 20)
        {
            uiManager.PopulateWin(3, SocketManager.resultData.WinAmout);
        }
        else
        {
            CheckPopups = false;
        }
    }

    internal void CheckBonusGame()
    {
        _bonusManager.StartBonus(SocketManager.tempBonus);
    }

    //generate the payout lines generated 
    private void CheckPayoutLineBackend(List<int> LineId, List<string> points_AnimString, List<string> BonusSymbols, List<string> ScatterSymbols)
    {
        List<int> y_points = null;
        List<int> points_anim = null;
        if (LineId.Count > 0 || points_AnimString.Count > 0 || BonusSymbols.Count > 0 || ScatterSymbols.Count > 0)
        {
            if (audioController) audioController.PlayWLAudio("win");
            for (int i = 0; i < LineId.Count; i++)
            {
                y_points = y_string[LineId[i] + 1]?.Split(',')?.Select(Int32.Parse)?.ToList();
                PayCalculator.GeneratePayoutLinesBackend(y_points, y_points.Count, LineId[i] % 10);
            }

            for (int i = 0; i < points_AnimString.Count; i++)
            {
                points_anim = points_AnimString[i]?.Split(',')?.Select(Int32.Parse)?.ToList();

                for (int k = 0; k < points_anim.Count; k++)
                {
                    if (points_anim[k] >= 10)
                    {
                        StartGameAnimation(Animimages[(points_anim[k] / 10) % 10].slotImages[points_anim[k] % 10].gameObject, TempBoxScripts[(points_anim[k] / 10) % 10].boxScripts[points_anim[k] % 10]);
                    }
                    else
                    {
                        StartGameAnimation(Animimages[0].slotImages[points_anim[k]].gameObject, TempBoxScripts[0].boxScripts[points_anim[k]]);
                    }
                }
            }

            if (BonusSymbols.Count > 0)
            {
                BonusSymbols = BonusSymbols.ConvertAll(item => item.Replace(",", ""));
                points_anim = null;
                for (int i = 0; i < BonusSymbols.Count; i++)
                {
                    points_anim = BonusSymbols[i]?.Split(',')?.Select(Int32.Parse)?.ToList();

                    for (int k = 0; k < points_anim.Count; k++)
                    {
                        if (points_anim[k] >= 10)
                        {
                            StartGameAnimation(Animimages[points_anim[k] % 10].slotImages[(points_anim[k] / 10) % 10].gameObject, TempBoxScripts[points_anim[k] % 10].boxScripts[(points_anim[k] / 10) % 10], true);
                        }
                        else
                        {
                            StartGameAnimation(Animimages[points_anim[k]].slotImages[0].gameObject, TempBoxScripts[points_anim[k]].boxScripts[0], true);
                        }
                    }
                }
            }

            if (ScatterSymbols.Count > 0)
            {
                ScatterSymbols = ScatterSymbols.ConvertAll(item => item.Replace(",", ""));
                points_anim = null;
                for (int i = 0; i < ScatterSymbols.Count; i++)
                {
                    points_anim = ScatterSymbols[i]?.Split(',')?.Select(Int32.Parse)?.ToList();

                    for (int k = 0; k < points_anim.Count; k++)
                    {
                        if (points_anim[k] >= 10)
                        {
                            StartGameAnimation(Animimages[points_anim[k] % 10].slotImages[(points_anim[k] / 10) % 10].gameObject, TempBoxScripts[points_anim[k] % 10].boxScripts[(points_anim[k] / 10) % 10], false, true);
                        }
                        else
                        {
                            StartGameAnimation(Animimages[points_anim[k]].slotImages[0].gameObject, TempBoxScripts[points_anim[k]].boxScripts[0], false, true);
                        }
                    }
                }
            }

            WinningsAnim(true);
        }
        else
        {
            if (audioController) audioController.StopWLAaudio();
        }
        if (LineId.Count > 0 || BonusSymbols.Count > 0 || ScatterSymbols.Count > 0)  
        {
            BoxAnimRoutine = StartCoroutine(BoxRoutine(LineId, BonusSymbols.Count, ScatterSymbols.Count));
        }
        CheckSpinAudio = false;
    }

    private IEnumerator BoxRoutine(List<int> LineIDs, int BonusCount, int scatterCount)
    {
        yield return new WaitForSeconds(2f);
        PayCalculator.DontDestroyLines.Clear();
        PayCalculator.DontDestroyLines.TrimExcess();
        PayCalculator.ResetLines();
        while (true)
        {
            List<int> y_points = null;
            if (LineIDs.Count > 0)
            {
                for (int i = 0; i < LineIDs.Count; i++)
                {
                    y_points = y_string[LineIDs[i] + 1]?.Split(',')?.Select(Int32.Parse)?.ToList();
                    PayCalculator.GeneratePayoutLinesBackend(y_points, y_points.Count, LineIDs[i] % 10);
                    PayCalculator.DontDestroyLines.Add(LineIDs[i]);
                    for (int s = 0; s < 5; s++)
                    {
                        if (TempBoxScripts[s].boxScripts[SocketManager.LineData[LineIDs[i]][s]].isAnim && !TempBoxScripts[s].boxScripts[SocketManager.LineData[LineIDs[i]][s]].isBonus && !TempBoxScripts[s].boxScripts[SocketManager.LineData[LineIDs[i]][s]].isScatter)
                        {
                            TempBoxScripts[s].boxScripts[SocketManager.LineData[LineIDs[i]][s]].SetBG(Box_Sprites[LineIDs[i] % 10]);
                            TempBoxScripts[s].boxScripts[SocketManager.LineData[LineIDs[i]][s]].transform.parent.gameObject.SetActive(true);
                        }
                    }
                    if (LineIDs.Count < 2)
                    {
                        yield break;
                    }
                    yield return new WaitForSeconds(2f);
                    for (int s = 0; s < 5; s++)
                    {
                        if (TempBoxScripts[s].boxScripts[SocketManager.LineData[LineIDs[i]][s]].isAnim && !TempBoxScripts[s].boxScripts[SocketManager.LineData[LineIDs[i]][s]].isBonus && !TempBoxScripts[s].boxScripts[SocketManager.LineData[LineIDs[i]][s]].isScatter)
                        {
                            TempBoxScripts[s].boxScripts[SocketManager.LineData[LineIDs[i]][s]].ResetBG();
                            TempBoxScripts[s].boxScripts[SocketManager.LineData[LineIDs[i]][s]].transform.parent.gameObject.SetActive(false);
                        }
                    }
                    PayCalculator.DontDestroyLines.Clear();
                    PayCalculator.DontDestroyLines.TrimExcess();
                    PayCalculator.ResetLines();
                }
            }

            if (BonusCount > 0)
            {
                for (int i = 0; i < TempList.Count; i++)
                {
                    if (TempList[i].GetComponent<BoxScripting>().isBonus)
                    {
                        TempList[i].transform.parent.gameObject.SetActive(true);
                    }
                }
                yield return new WaitForSeconds(2f);

                for (int i = 0; i < TempList.Count; i++)
                {
                    if (TempList[i].GetComponent<BoxScripting>().isBonus)
                    {
                        TempList[i].transform.parent.gameObject.SetActive(false);
                    }
                }
            }

            if (scatterCount > 0)
            {
                for (int i = 0; i < TempList.Count; i++)
                {
                    if (TempList[i].GetComponent<BoxScripting>().isScatter)
                    {
                        TempList[i].transform.parent.gameObject.SetActive(true);
                    }
                }
                yield return new WaitForSeconds(2f);

                for (int i = 0; i < TempList.Count; i++)
                {
                    if (TempList[i].GetComponent<BoxScripting>().isScatter)
                    {
                        TempList[i].transform.parent.gameObject.SetActive(false);
                    }
                }
            }

            for (int i = 0; i < LineIDs.Count; i++)
            {
                y_points = y_string[LineIDs[i] + 1]?.Split(',')?.Select(Int32.Parse)?.ToList();
                PayCalculator.GeneratePayoutLinesBackend(y_points, y_points.Count, LineIDs[i] % 10);
                PayCalculator.DontDestroyLines.Add(LineIDs[i]);
            }
            yield return new WaitForSeconds(2f);
            PayCalculator.DontDestroyLines.Clear();
            PayCalculator.DontDestroyLines.TrimExcess();
            PayCalculator.ResetLines();
        }
    }

    private void WinningsAnim(bool IsStart)
    {
        if (IsStart)
        {
            WinTween = TotalWin_text.gameObject.GetComponent<RectTransform>().DOScale(new Vector2(1.5f, 1.5f), 1f).SetLoops(-1, LoopType.Yoyo).SetDelay(0);
        }
        else
        {
            WinTween.Kill();
            TotalWin_text.gameObject.GetComponent<RectTransform>().localScale = Vector3.one;
        }
    }

    #endregion

    internal void CallCloseSocket()
    {
        SocketManager.CloseSocket();
    }


    void ToggleButtonGrp(bool toggle)
    {

        if (SlotStart_Button) SlotStart_Button.interactable = toggle;
        if (MaxBet_Button) MaxBet_Button.interactable = toggle;
        if (BetMinus_Button) BetMinus_Button.interactable = toggle;
        if (BetPlus_Button) BetPlus_Button.interactable = toggle;
        if (TBPlus_Button) TBPlus_Button.interactable = toggle;
        if (TBMinus_Button) TBMinus_Button.interactable = toggle;
        if (Settings_Button) Settings_Button.interactable = toggle;
    }

    //start the icons animation
    private void StartGameAnimation(GameObject animObjects, BoxScripting boxscript, bool isBonus = false, bool isScatter = false)
    {
        ImageAnimation temp = animObjects.GetComponent<ImageAnimation>();
        temp.StartAnimation();
        TempList.Add(temp);
        boxscript.isAnim = true;
        boxscript.isBonus = isBonus;
        boxscript.isScatter = isScatter;
    }

    //stop the icons animation
    private void StopGameAnimation()
    {
        for (int i = 0; i < TempList.Count; i++)
        {
            TempList[i].StopAnimation();
        }

        if (BoxAnimRoutine != null)
        {
            StopCoroutine(BoxAnimRoutine);
            BoxAnimRoutine = null;
        }
        for (int i = 0; i < TempBoxScripts.Count; i++)
        {
            foreach (BoxScripting b in TempBoxScripts[i].boxScripts)
            {
                b.isAnim = false;
                b.isScatter = false;
                b.isBonus = false;
                b.ResetBG();
                b.transform.parent.gameObject.SetActive(false);
            }
        }
        TempList.Clear();
        TempList.TrimExcess();
    }


    #region TweeningCode
    private void InitializeTweening(Transform slotTransform)
    {
        slotTransform.localPosition = new Vector2(slotTransform.localPosition.x, 0);
        Tweener tweener = slotTransform.DOLocalMoveY(-tweenHeight, 0.2f).SetLoops(-1, LoopType.Restart).SetDelay(0).SetEase(Ease.Linear);
        tweener.Play();
        alltweens.Add(tweener);
    }



    private IEnumerator StopTweening(int reqpos, Transform slotTransform, int index)
    {
        alltweens[index].Pause();
        int tweenpos = (reqpos * IconSizeFactor) - IconSizeFactor;
        alltweens[index] = slotTransform.DOLocalMoveY(-tweenpos + 100, 0.5f).SetEase(Ease.OutElastic);
        yield return new WaitForSeconds(0.2f);
    }


    private void KillAllTweens()
    {
        for (int i = 0; i < numberOfSlots; i++)
        {
            alltweens[i].Kill();
        }
        alltweens.Clear();

    }
    #endregion

}

[Serializable]
public class SlotImage
{
    public List<Image> slotImages = new List<Image>(10);
}

[Serializable]
public class BoxScript
{
    public List<BoxScripting> boxScripts = new List<BoxScripting>(10);
}

