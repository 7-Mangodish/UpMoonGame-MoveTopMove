using System;
using System.Threading.Tasks;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SocialPlatforms.Impl;

public class StateManager : MonoBehaviour
{

    private int currentScore;
    public int CurrentScore { get { return currentScore; } }
    private int addingScore;
    public bool IsLevelUp;

    [Header("Weapon's State")]
    [SerializeField] private float deltaScaleWeapon;
    [SerializeField] private GameObject maxDistancePoint;
    private ThrowWeapon.StateWeapon stateWeapon;

    [Header("Character's Control")]
    private AnimationControl animationControl;

    [Header("Player's Scale")]
    [SerializeField] private TextMeshProUGUI playerScaleText;
    private int currentLevel;


    public event EventHandler<int> OnCharacterTakeScore;
    public event EventHandler OnCharacterDead;

    public class OnCharacterLevelUpArg : EventArgs {
        public float currentLevel;
        public float deltaPositionY;
    }
    public event EventHandler<OnCharacterLevelUpArg> OnCharacterLevelUp;

    public bool isDead = false;
    private void Awake() {
        addingScore = 1;
        currentScore = 0;
        currentLevel = 1;
        if(maxDistancePoint != null) {
            stateWeapon.ownerStateManager = this;
            stateWeapon.maxDistance = Vector3.Distance(this.transform.position, maxDistancePoint.transform.position);
            stateWeapon.curScale = 0;
            deltaScaleWeapon = 3;
        }
        else {
            Debug.Log("maxDistnance is null in " + this.gameObject.name);
        }

        animationControl  = GetComponent<AnimationControl>();
    }
    public void AddScore() {
        if (isDead) return;

        this.currentScore += addingScore;
        OnCharacterTakeScore?.Invoke(this, this.currentScore);



        if (currentScore%2 == 0 && currentScore !=0) {
            IsLevelUp = true;
            currentLevel++;

            animationControl.PlayLevelUpEff();
            OnCharacterLevelUp?.Invoke(this, new OnCharacterLevelUpArg {
                currentLevel = this.currentLevel,
                deltaPositionY = 0.05f
            });

            //Cap nhat scale cua nhan vat, cap nhat tam danh va scale cua vu khi
            this.transform.localScale += new Vector3(0.05f, 0.05f, 0.05f);
            if (this.gameObject.CompareTag("Player")) {
                DisplayPlayerScale(this.transform.localScale.x * 5f);
            }


            stateWeapon.curScale += deltaScaleWeapon;
            stateWeapon.maxDistance = Vector3.Distance(this.transform.position, maxDistancePoint.transform.position);

            // Cap nhat cammera
            if (this.gameObject.CompareTag("Player"))
                CameraMove.Instance.UpdateCamera(.75f);
        }
    }
    
    private void UpdateCameraPosition() {
        CinemachinePositionComposer cam = FindFirstObjectByType<CinemachinePositionComposer>();
        cam.CameraDistance += .75f;
    }

    public ThrowWeapon.StateWeapon GetStateWeapon() {
        return stateWeapon;
    }

    public void TriggerCharacterDead() {
        this.isDead = true;

        //Animation
        OnCharacterDead?.Invoke(this, EventArgs.Empty);
        animationControl.SetDead();


        //Destroy Gameobject
        if (this.gameObject.CompareTag("Enemy")) {
            Destroy(this.transform.parent.gameObject, 2);
        }
        else
            Destroy(this.gameObject, 2);
    }

    public async void DisplayPlayerScale(float playerScale) {
        playerScaleText.gameObject.SetActive(true);
        playerScaleText.text = playerScale.ToString() + "m";
        playerScaleText.GetComponent<Animator>().Play("PlayerScaleText");
        await Task.Delay(1500);
        playerScaleText.gameObject.SetActive(false);
    }
}
