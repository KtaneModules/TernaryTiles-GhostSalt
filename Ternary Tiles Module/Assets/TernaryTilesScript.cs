using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class TernaryTilesScript : MonoBehaviour
{

    static int _moduleIdCounter = 1;
    int _moduleID = 0;

    public KMBombModule Module;
    public KMBombInfo Bomb;
    public KMAudio Audio;
    public KMSelectable[] Buttons;
    public KMSelectable SubmitButton;
    public Material[] Mats;
    public TextMesh[] ButtonTexts;
    public TextMesh IndicatorText;

    private int[] ButtonValues = new int[12];
    private int SecretPresses;
    private string IndicatorChars = "ABC1234+";
    private string NumsZeroToTwo = "012";
    private string IndicatorTextVariable;
    private bool Solved;

    void Awake()
    {
        _moduleID = _moduleIdCounter++;
        for (int i = 0; i < 12; i++)
        {
            int x = i;
            Buttons[x].OnInteract += delegate { StartCoroutine(ButtonPress(x)); return false; };
            Buttons[x].GetComponent<MeshRenderer>().material = Mats[0];
        }
        SubmitButton.OnInteract += delegate { StartCoroutine(Submit()); return false; };
        for (int i = 0; i < 12; i++)
        {
            IndicatorTextVariable += IndicatorChars[Rnd.Range(0, 8)];
        }
        StartCoroutine(IndicatorDisplay());
    }

    // Use this for initialization
    void Start()
    {
        Calculate();
    }

    void Calculate()
    {
        if (Bomb.IsPortPresent(Port.Serial) || Bomb.IsPortPresent(Port.AC))
        {
            ButtonValues[8]++;
        }
        if (Bomb.IsPortPresent(Port.Parallel) || Bomb.IsPortPresent(Port.USB))
        {
            ButtonValues[4]++;
        }
        if (Bomb.IsPortPresent(Port.RJ45) || Bomb.IsPortPresent(Port.ComponentVideo))
        {
            ButtonValues[7]++;
        }
        if (Bomb.IsPortPresent(Port.DVI) || Bomb.IsPortPresent(Port.CompositeVideo))
        {
            ButtonValues[9]++;
        }
        if (Bomb.IsPortPresent(Port.PS2) || Bomb.IsPortPresent(Port.HDMI))
        {
            ButtonValues[6]++;
        }
        if (Bomb.IsPortPresent(Port.StereoRCA) || Bomb.IsPortPresent(Port.PCMCIA))
        {
            ButtonValues[2]++;
        }
        if (Bomb.GetSerialNumberNumbers().Last() % 2 == 1)
        {
            ButtonValues[1]++;
        }
        if (Bomb.GetSerialNumberNumbers().First() % 2 == 0)
        {
            ButtonValues[10]++;
        }
        if (Bomb.GetSerialNumberLetters().Any(x => x == 'T' || x == 'I' || x == 'L' || x == 'E' || x == 'S'))
        {
            ButtonValues[0]++;
        }
        if (Bomb.GetOnIndicators().Count() > Bomb.GetOffIndicators().Count())
        {
            ButtonValues[5]++;
        }
        if (Bomb.IsIndicatorOn("MSA") || Bomb.IsIndicatorOn("NSA"))
        {
            ButtonValues[11]++;
        }
        if (Bomb.IsIndicatorOff("CAR") || Bomb.IsIndicatorOff("CLR"))
        {
            ButtonValues[3]++;
        }
        Debug.LogFormat("[Ternary Tiles #{0}] The base values in reading order are {1}.", _moduleID, ButtonValues.Join(", "));
        for (int i = 0; i < 12; i++)
        {
            switch (IndicatorTextVariable[i])
            {
                case 'A':
                    ButtonValues[0] = (ButtonValues[0] + 1) % 3;
                    ButtonValues[3] = (ButtonValues[3] + 1) % 3;
                    ButtonValues[6] = (ButtonValues[6] + 1) % 3;
                    ButtonValues[9] = (ButtonValues[9] + 1) % 3;
                    break;
                case 'B':
                    ButtonValues[1] = (ButtonValues[1] + 1) % 3;
                    ButtonValues[4] = (ButtonValues[4] + 1) % 3;
                    ButtonValues[7] = (ButtonValues[7] + 1) % 3;
                    ButtonValues[10] = (ButtonValues[10] + 1) % 3;
                    break;
                case 'C':
                    ButtonValues[2] = (ButtonValues[2] + 1) % 3;
                    ButtonValues[5] = (ButtonValues[5] + 1) % 3;
                    ButtonValues[8] = (ButtonValues[8] + 1) % 3;
                    ButtonValues[11] = (ButtonValues[11] + 1) % 3;
                    break;
                case '1':
                    ButtonValues[0] = (ButtonValues[0] + 1) % 3;
                    ButtonValues[1] = (ButtonValues[1] + 1) % 3;
                    ButtonValues[2] = (ButtonValues[2] + 1) % 3;
                    break;
                case '2':
                    ButtonValues[3] = (ButtonValues[3] + 1) % 3;
                    ButtonValues[4] = (ButtonValues[4] + 1) % 3;
                    ButtonValues[5] = (ButtonValues[5] + 1) % 3;
                    break;
                case '3':
                    ButtonValues[6] = (ButtonValues[6] + 1) % 3;
                    ButtonValues[7] = (ButtonValues[7] + 1) % 3;
                    ButtonValues[8] = (ButtonValues[8] + 1) % 3;
                    break;
                case '4':
                    ButtonValues[9] = (ButtonValues[9] + 1) % 3;
                    ButtonValues[10] = (ButtonValues[10] + 1) % 3;
                    ButtonValues[11] = (ButtonValues[11] + 1) % 3;
                    break;
                default:
                    for (int j = 0; j < 12; j++)
                    {
                        ButtonValues[j] = (ButtonValues[j] + 1) % 3;
                    }
                    break;
            }
        }
        Debug.LogFormat("[Ternary Tiles #{0}] The final values in reading order are {1}.", _moduleID, ButtonValues.Join(", "));
    }

    private IEnumerator ButtonPress(int pos)
    {
        Buttons[pos].AddInteractionPunch(0.5f);
        Audio.PlaySoundAtTransform("press", Buttons[pos].transform);
        for (int i = 0; i < 3; i++)
        {
            Buttons[pos].transform.localPosition -= new Vector3(0, 0.0025f, 0);
            yield return new WaitForSeconds(0.01f);
        }
        if (!Solved)
        {
            Buttons[pos].GetComponent<MeshRenderer>().material = Mats[(NumsZeroToTwo.IndexOf(ButtonTexts[pos].text) + 1) % 3];
            ButtonTexts[pos].text = ((NumsZeroToTwo.IndexOf(ButtonTexts[pos].text) + 1) % 3).ToString();
        }
        for (int i = 0; i < 3; i++)
        {
            Buttons[pos].transform.localPosition += new Vector3(0, 0.0025f, 0);
            yield return new WaitForSeconds(0.01f);
        }
    }

    private IEnumerator Submit()
    {
        SubmitButton.AddInteractionPunch(2f);
        for (int i = 0; i < 3; i++)
        {
            SubmitButton.transform.localPosition -= new Vector3(0, 0.001f, 0);
            yield return new WaitForSeconds(0.01f);
        }
        if (!Solved)
        {
            bool Strike = false;
            string TempLog = "";
            for (int i = 0; i < 12; i++)
            {
                if (ButtonTexts[i].text != ButtonValues[i].ToString())
                {
                    Strike = true;
                }
                TempLog += ButtonTexts[i].text + ", ";
            }
            if (Strike)
            {
                Module.HandleStrike();
                Audio.PlaySoundAtTransform("strike", SubmitButton.transform);
                Debug.LogFormat("[Ternary Tiles #{0}] You submitted {1}which was incorrect. Strike!", _moduleID, TempLog);
            }
            else
            {
                Module.HandlePass();
                Audio.PlaySoundAtTransform("solve", SubmitButton.transform);
                Solved = true;
                IndicatorText.text = "Module solved! :D";
                Debug.LogFormat("[Ternary Tiles #{0}] You submitted {1}which was correct. Module solved!", _moduleID, TempLog);
            }
        }
        else
        {
            SecretPresses++;
            if (SecretPresses % 50 == 0 && SecretPresses != 0)
            {
                Audio.PlaySoundAtTransform("secret", SubmitButton.transform);
            }
        }
        for (int i = 0; i < 3; i++)
        {
            SubmitButton.transform.localPosition += new Vector3(0, 0.001f, 0);
            yield return new WaitForSeconds(0.01f);
        }
    }

    private IEnumerator IndicatorDisplay()
    {
        while (!Solved)
        {
            for (int i = 0; i < 12; i++)
            {
                if (!Solved)
                {
                    IndicatorText.text += IndicatorTextVariable[i];
                }
                else
                {
                    IndicatorText.text = "Module solved! :D";
                }
                yield return new WaitForSeconds(0.25f);
            }
            for (int i = 0; i < 3; i++)
            {
                yield return new WaitForSeconds(0.25f);
                if (!Solved)
                {
                    IndicatorText.text = IndicatorTextVariable;
                }
                else
                {
                    IndicatorText.text = "Module solved! :D";
                }
                yield return new WaitForSeconds(0.25f);
                if (!Solved)
                {
                    IndicatorText.text = "";
                }
                else
                {
                    IndicatorText.text = "Module solved! :D";
                }
            }
        }
        IndicatorText.text = "Module solved! :D";
    }

#pragma warning disable 414
    private string TwitchHelpMessage = "Use '!{0} A1 A2 submit' to press A1, A2, then the submit button.";
#pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.ToLowerInvariant();
        string[] CommandArray = command.Split(' ');
        string[] ValidCommands = { "a1", "b1", "c1", "a2", "b2", "c2", "a3", "b3", "c3", "a4", "b4", "c4" };
        for (int i = 0; i < CommandArray.Length; i++)
        {
            if (!ValidCommands.Contains(CommandArray[i]))
            {
                if (CommandArray[i] != "submit")
                {
                    yield return "sendtochaterror Invalid command.";
                    yield break;
                }
                else
                {
                    yield return null;
                    SubmitButton.OnInteract();
                    yield break;
                }
            }
            else
            {
                for (int j = 0; j < 12; j++)
                {
                    if (ValidCommands[j] == CommandArray[i])
                    {
                        Buttons[j].OnInteract();
                        yield return new WaitForSeconds(0.1f);
                    }
                }
            }
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        for (int i = 0; i < 12; i++)
        {
            while (ButtonTexts[i].text != ButtonValues[i].ToString())
            {
                Buttons[i].OnInteract();
                yield return true;
            }
        }
        yield return new WaitForSeconds(0.1f);
        SubmitButton.OnInteract();
    }
}
