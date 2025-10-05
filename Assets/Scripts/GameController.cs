using System;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public GameObject canvas;
    public float gameTime = 60f;
    public TMP_Text gameTimeText;
    public TMP_Text gameTimeShadowText;

    private float gameSpeed = 0f;
    public float maxGameSpeed = 5f;

    public float driveAcceleration;
    public float breakFactor;

    public GameObject[] wheels;
    public float wheelRotateSpeed;

    public GameObject claw;
    private Animator clawAnim;
    public Transform clawCenter;

    private bool isBreaking = false;

    public Vector2 timeBetweenCans;
    private float canSpawnTime = 0f;
    public GameObject[] cans;
    public float ensureProbability;

    private Action spaceStart;
    private Action spaceCancel;

    private float initialSpeedOfDecay = 0f;

    private bool doLift = false;

    public TMP_Text scoreText;
    public TMP_Text scoreShadowText;

    public AudioSource engineSource;
    public Vector2 engineMinMaxVolume;
    public AudioSource sfxSource;
    public AudioClip[] scoreSFX;
    public AudioClip[] missedSFX;
    public AudioClip[] liftSFX;
    public AudioClip[] grabSFX;
    public AudioClip[] ensureSFX;
    public AudioClip[] dropSFX;
    public AudioClip[] releaseSFX;

    public GameObject updateValueTextPrefab;
    public int score { get; private set; }

    void Start()
    {
        if (InputManager.Instance != null)
        {
            spaceStart = () => { isBreaking = true; initialSpeedOfDecay = gameSpeed; };
            spaceCancel = () => { isBreaking = false; };
            InputManager.Instance.OnSpaceStarted += spaceStart;
            InputManager.Instance.OnSpaceCanceled += spaceCancel;
        }

        clawAnim = claw.GetComponent<Animator>();
    }

    void OnDestroy()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnSpaceStarted -= spaceStart;
            InputManager.Instance.OnSpaceCanceled -= spaceCancel;
        }
    }

    private void UpdateGameSpeed()
    {
        if (isBreaking)
        {
            if (gameSpeed > .1f) gameSpeed *= breakFactor;
            else gameSpeed = 0f;
        }
        else
        {
            gameSpeed += driveAcceleration * Time.deltaTime * Time.deltaTime;
        }

        gameSpeed = Mathf.Clamp(gameSpeed, 0, maxGameSpeed);
    }

    private void RotateWheels()
    {
        foreach (GameObject wheel in wheels)
        {
            wheel.transform.Rotate(-Vector3.forward * gameSpeed * wheelRotateSpeed);
        }
    }

    int nextCan = 0;
    private void HandleCans()
    {
        for (int i = 0; i < cans.Length; i++)
        {
            GameObject can = cans[i];
            if (can.transform.position.x > -9.5f)
            {
                can.transform.position -= Vector3.right * gameSpeed * Time.deltaTime;
            }
            else if (can.CompareTag("Full"))
            {
                can.tag = "Untagged";
                gameTime -= 1f;
                sfxSource.PlayOneShot(missedSFX[UnityEngine.Random.Range(0, missedSFX.Length)]);

                Instantiate(updateValueTextPrefab, canvas.transform).GetComponent<ValueChangeText>().Initialize(canvas.transform.position + new Vector3(120f, 325f, 0f) * .5f, Vector3.down, 2f, Vector3.right, "-1", 48);
            }
        }

        if (canSpawnTime <= 0f && cans[nextCan].transform.position.x <= -9.5f)
        {
            cans[nextCan].transform.position = new Vector3(9.5f, cans[nextCan].transform.position.y, cans[nextCan].transform.position.z);
            cans[nextCan].tag = "Full";
            nextCan = (nextCan + 1) % cans.Length;

            canSpawnTime = UnityEngine.Random.Range(timeBetweenCans.x, timeBetweenCans.y);
        }
        else if (canSpawnTime > 0f)
        {
            canSpawnTime -= Time.deltaTime * (gameSpeed / maxGameSpeed);
        }
    }

    GameObject grabbedCan = null;
    int assignScore = 0;
    int playLiftSFX = 0;
    int playEnsureSFX = 0;
    int playDropSFX = 0;
    int playReleaseSFX = 0;
    int scoreSFXIdx = 0;
    private void AttemptLift()
    {
        if (grabbedCan) {
            float nt = clawAnim.GetCurrentAnimatorStateInfo(0).normalizedTime;
            if (clawAnim.GetCurrentAnimatorStateInfo(0).IsName("Lift"))
            {
                playReleaseSFX = 0;
                if (.2f < nt && playLiftSFX == 0)
                {
                    sfxSource.PlayOneShot(liftSFX[UnityEngine.Random.Range(0, liftSFX.Length)]);
                    playLiftSFX++;
                }
                if (nt > .7f && assignScore == 0)
                {
                    int deltaScore = Mathf.Max((int)(initialSpeedOfDecay * 2f + .5f), 1);
                    scoreSFXIdx = UnityEngine.Random.Range(0, scoreSFX.Length);
                    score += deltaScore;
                    if (gameTime > 0f) gameTime += 10f;

                    assignScore++;
                    sfxSource.PlayOneShot(scoreSFX[scoreSFXIdx], 1f);

                    Instantiate(updateValueTextPrefab, canvas.transform).GetComponent<ValueChangeText>().Initialize(canvas.transform.position + new Vector3(-400f, 490.25f, 0f) * .5f, Vector3.up, 2f, Vector3.right, "+" + deltaScore + "00", 24);
                    Instantiate(updateValueTextPrefab, canvas.transform).GetComponent<ValueChangeText>().Initialize(canvas.transform.position + new Vector3(120f, 325f, 0f) * .5f, Vector3.up, 2f, Vector3.right, "+10", 48);
                }
            }
            else if (clawAnim.GetCurrentAnimatorStateInfo(0).IsName("Ensure"))
            {
                if (nt > .5f && playEnsureSFX == 0)
                {
                    sfxSource.PlayOneShot(ensureSFX[UnityEngine.Random.Range(0, ensureSFX.Length)]);
                    playEnsureSFX++;
                }
                if (nt > .7f && assignScore == 1)
                {
                    score += 1;
                    if (gameTime > 0f) gameTime += 1f;
                    assignScore += 1;
                    sfxSource.PlayOneShot(scoreSFX[scoreSFXIdx], 1f);

                    Instantiate(updateValueTextPrefab, canvas.transform).GetComponent<ValueChangeText>().Initialize(canvas.transform.position + new Vector3(-400f, 490.25f, 0f) * .5f, Vector3.up, 2f, Vector3.right, "+100", 24);
                    Instantiate(updateValueTextPrefab, canvas.transform).GetComponent<ValueChangeText>().Initialize(canvas.transform.position + new Vector3(120f, 325f, 0f) * .5f, Vector3.up, 2f, Vector3.right, "+1", 48);
                }
            }
            else if (clawAnim.GetCurrentAnimatorStateInfo(0).IsName("Drop"))
            {   
                if (nt < .2f && playDropSFX == 0)
                {
                    sfxSource.PlayOneShot(dropSFX[UnityEngine.Random.Range(0, dropSFX.Length)]);
                    playDropSFX++;
                }
                if (nt > .9f && playReleaseSFX == 0)
                {
                    grabbedCan.SetActive(true);
                    grabbedCan = null;
                    doLift = false;
                    assignScore = 0;

                    sfxSource.PlayOneShot(releaseSFX[UnityEngine.Random.Range(0, releaseSFX.Length)], 1f);
                    playLiftSFX = 0;
                    playEnsureSFX = 0;
                    playDropSFX = 0;
                    return;
                }
            }
        }

        if (grabbedCan || gameSpeed > 0f) return;

        for (int i = 0; i < cans.Length; i++)
        {
            GameObject can = cans[i];
            if (can.CompareTag("Full") && Mathf.Abs(can.transform.position.x - clawCenter.transform.position.x) < 5f / 16f)
            {
                clawAnim.SetBool("DoEnsure", UnityEngine.Random.Range(0f, 1f) < ensureProbability);
                clawAnim.SetTrigger("LiftTrigger");
                doLift = true;

                can.SetActive(false);
                can.transform.position = clawCenter.transform.position;
                can.tag = "Empty";

                sfxSource.PlayOneShot(grabSFX[UnityEngine.Random.Range(0, grabSFX.Length)]);

                grabbedCan = can;
                break;
            }
        }
    }

    private void UpdateScore()
    {
        scoreText.text = "SCORE: " + score + "00";
        scoreShadowText.text = "SCORE: " + score + "00";
    }

    private void UpdateEngineSound()
    {
        float p = gameSpeed / maxGameSpeed;
        engineSource.volume = engineMinMaxVolume.x * (1f - p) + engineMinMaxVolume.y * p;
    }

    private void UpdateGameTimer()
    {
        gameTime -= Time.deltaTime;
        gameTimeText.text = ((int)(gameTime + .99f)).ToString();
        gameTimeShadowText.text = ((int)(gameTime + .99f)).ToString();

    }

    // Update is called once per frame
    void Update()
    {
        if (-2f <= gameTime && gameTime <= 0f)
        {
            isBreaking = true;
            gameTime -= Time.deltaTime;
        }
        else if (gameTime < -2f)
        {
            PlayerPrefs.SetInt("Score", score);
            if (score > PlayerPrefs.GetInt("Highscore"))
            {
                PlayerPrefs.SetInt("Highscore", score);
            }

            SceneManager.LoadScene(2);
        }

        AttemptLift();
        if (!doLift)
        {
            UpdateGameSpeed();
        }
        HandleCans();
        RotateWheels();
        UpdateEngineSound();
        UpdateScore();
        if (gameTime > 0f)
        {
            UpdateGameTimer();
        }
    }
}
