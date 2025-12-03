using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CutsceneTriggerer : MonoBehaviour
{
    [SerializeField]
    GameObject kitty, train, cam, door, sceneFader;

    Vector3 camTargetPos, catTargetPos;
    float followRatio;

    private Animator catAnim, doorAnim, trainAnim;
    SceneFadeIn sceneFade;

    private int state = -1;
    private float currentTime;

    private Vector3 trainOffset = new Vector3(0,2f,31.5f);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    
    void Update()
    {
        Vector3 nextCatPos;
        switch (state)
        {
            case 0:
                cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation, Quaternion.LookRotation(catTargetPos + Vector3.up*2 - cam.transform.position), 0.8f * 20 * Time.deltaTime);
                cam.transform.position = Vector3.Lerp(cam.transform.position, camTargetPos, followRatio * Time.deltaTime);
                kitty.transform.rotation = Quaternion.Slerp(kitty.transform.rotation, Quaternion.Euler(0,90,0), 0.8f * Time.deltaTime);
                nextCatPos = Vector3.Lerp(kitty.transform.position, catTargetPos, followRatio * 2 * Time.deltaTime);
                // Debug.Log((nextCatPos - kitty.transform.position).magnitude);
                catAnim.SetFloat("State", (nextCatPos - kitty.transform.position).magnitude * 1000f);
                kitty.transform.position = nextCatPos;
                currentTime += Time.deltaTime;
                if((cam.transform.position - camTargetPos).magnitude < 0.01f && (kitty.transform.position - catTargetPos).magnitude < 0.01f && currentTime > 3f)
                {
                    state = 1;
                    currentTime = 0;
                }
                break;
            case 1:
                doorAnim.SetTrigger("FadeDoorIn");
                currentTime += Time.deltaTime;
                if(currentTime > 1.5f)
                {
                    state = 2;
                    currentTime = 0;
                    catTargetPos = new Vector3(42.881f, 8f, -142.838f);
                    followRatio = 1.5f;
                }
                break;
            case 2:
                cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation, Quaternion.LookRotation(catTargetPos + Vector3.up - cam.transform.position), 0.8f * 20 * Time.deltaTime);
                cam.transform.position = Vector3.Lerp(cam.transform.position, camTargetPos, followRatio * Time.deltaTime);
                kitty.transform.rotation = Quaternion.Slerp(kitty.transform.rotation, Quaternion.Euler(0,90,0), 0.8f * Time.deltaTime);
                nextCatPos = Vector3.Lerp(kitty.transform.position, catTargetPos, followRatio * 2 * Time.deltaTime);
                // Debug.Log((nextCatPos - kitty.transform.position).magnitude);
                catAnim.SetFloat("State", (nextCatPos - kitty.transform.position).magnitude * 1000f);
                kitty.transform.position = nextCatPos;
                currentTime += Time.deltaTime;
                if((cam.transform.position - camTargetPos).magnitude < 0.01f && (kitty.transform.position - catTargetPos).magnitude < 0.01f && currentTime > 1f)
                {
                    state = 3;
                    currentTime = 0;
                    catTargetPos = new Vector3(42.881f, 8f, -142.838f);
                    followRatio = 1.5f;
                }
                break;
            case 3:
                doorAnim.SetTrigger("FadeDoorOut");
                currentTime += Time.deltaTime;
                if(currentTime > 1.5f)
                {
                    state = 4;
                    currentTime = 0;
                    catTargetPos = new Vector3(42.881f, 8f, -142.838f);
                    followRatio = 1.5f;
                }
                break;
            case 4:
                trainAnim.SetTrigger("MoveTrain");
                // Debug.DrawRay(cam.transform.position, train.transform.position + trainOffset - cam.transform.position, Color.aliceBlue, 20f);
                cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation, Quaternion.LookRotation(train.transform.position + trainOffset - cam.transform.position), 0.8f * 20 * Time.deltaTime);
                currentTime += Time.deltaTime;
                if(currentTime > 5f)
                {
                    state = 5;
                    currentTime = 0;
                }
                break;
            case 5:
                if(sceneFader != null) {
                    sceneFader.SetActive(true);
                    sceneFade.Fade();
                }
                sceneFader = null;
                currentTime += Time.deltaTime;
                if(currentTime > 4f)
                {
                    SceneManager.LoadScene("MainMenu");
                }
                break;
            default:
                break;
        }
    }
    

    void OnTriggerEnter(Collider other)
    {
        catAnim = kitty.GetComponent<Animator>();
        doorAnim = door.GetComponent<Animator>();
        trainAnim = train.GetComponent<Animator>();
        sceneFade = sceneFader.GetComponent<SceneFadeIn>();
        AudioController ac = kitty.GetComponent<AudioController>();
        ac.enabled = false;
        int HIGH_CLIP = 2;
        ac.SwitchToClip(HIGH_CLIP);
        Destroy(kitty.GetComponent<PlayerController>());
        Destroy(cam.GetComponent<CameraLook>());
        camTargetPos = new Vector3(33.881f, 8, -142.838f);
        catTargetPos = new Vector3(40.881f, 5.75f, -142.838f);
        followRatio = 1.5f;
        state = 0;
        currentTime = 0;
    }
}
