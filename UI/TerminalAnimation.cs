using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class TerminalAnimation : MonoBehaviour {
    public AudioSource audioSource;
    public GameObject terminalEntryPrefab;
    public Transform container;
    public Color defaultColor;
    [Header("sounds")]
    public AudioClip[] typingSounds;
    TerminalEntry lastEntry;
    Coroutine currentCoroutine;
    void Start() {
        Clear();
    }
    public Coroutine DoWriteMany(params Writeln[] inputs) {
        if (currentCoroutine != null) {
            StopCoroutine(currentCoroutine);
            Clear();
        }
        currentCoroutine = StartCoroutine(WriteMany(inputs));
        return currentCoroutine;
    }
    public IEnumerator WriteMany(params Writeln[] inputs) {
        IEnumerator[] routines = inputs.Select((Writeln input) => CreateEntry(input)).ToArray();
        return Toolbox.ChainCoroutines(routines);
    }

    IEnumerator CreateEntry(Writeln input) {
        yield return null;
        GameObject obj = GameObject.Instantiate(terminalEntryPrefab);
        obj.transform.SetParent(container, false);
        lastEntry = obj.GetComponent<TerminalEntry>();
        lastEntry.audioSource = audioSource;
        if (input.destroyAfter > 0) {
            Destroy(obj, input.destroyAfter);
        }
        yield return lastEntry.Write(input, typingSounds);
    }
    public void Clear() {
        foreach (Transform child in container) {
            Destroy(child.gameObject);
        }
    }

    public void ShowThenPrompt(IEnumerator show, CyberNode target) {
        if (currentCoroutine != null) {
            StopCoroutine(currentCoroutine);
        }
        currentCoroutine = StartCoroutine(Toolbox.ChainCoroutines(show, DrawBasicNodePrompt(target)));
    }
    public IEnumerator DrawBasicNodePrompt(CyberNode node) {
        IEnumerator write = null;
        if (node.visibility == NodeVisibility.unknown || node.visibility == NodeVisibility.mystery) {
            // terminalContent.text = "> ping 127.0.5.10\nPING 127.0.5.10: 56 data bytes\n64 bytes from 127.0.5.10: icmp_seq=0 ttl=64 time=0.118 ms";
            write = WriteMany(new Writeln[]{
                    new Writeln("bash>", "ping 127.0.5.10", defaultColor, playerType:true),
                    new Writeln("", "PING 127.0.5.10: 56 data bytes", defaultColor),
                    new Writeln("", "PING 127.0.5.10: 56 data bytes", defaultColor),
                });
        } else {
            // target is visible / mapped
            if (node.getStatus() == CyberNodeStatus.compromised) {
                // terminalContent.text = "root @ 127.0.05.10 > command? █";
                write = WriteMany(new Writeln[]{
                    new Writeln("root @ 127.0.05.10 >", "command? █", defaultColor)
                });
            } else {
                if (node.lockLevel > 0) {
                    write = WriteMany(new Writeln[]{
                        new Writeln("bash>", "ssh 127.0.5.10", defaultColor, playerType:true),
                        new Writeln("", "* welcome to city sense/net! *", defaultColor),
                        new Writeln("", "new users must register with sysadmin.", defaultColor),
                        new Writeln("", "    enter password:█", defaultColor),
                     });
                    // terminalContent.text = "> ssh 127.0.5.10\n* welcome to city sense/net! *\nnew users must register with sysadmin.\n\n    enter password:█";
                } else {
                    // terminalContent.text = "> ssh 127.0.5.10\n* welcome to city sense/net! *\nnew users must register with sysadmin.\n\n    enter password:*****\nACCESS GRANTED\n\nbash>█";
                    write = WriteMany(new Writeln[]{
                            new Writeln("bash>", "ssh 127.0.5.10", defaultColor, true),
                            new Writeln("", "* welcome to city sense/net! *", defaultColor),
                            new Writeln("", "new users must register with sysadmin.", defaultColor),
                            new Writeln("", "    enter password:*****", defaultColor),
                            new Writeln("", "ACCESS GRANTED", defaultColor),
                            new Writeln("bash>", "█", defaultColor),
                        });
                }
            }
        }
        return write;
    }

    public void HandleSoftwareCallback(SoftwareState state) {
        if (currentCoroutine != null) {
            StopCoroutine(currentCoroutine);
        }
        IEnumerator write = WriteMany(new Writeln[]{
                            new Writeln("bash>", state.template.name+".exe", defaultColor, playerType:true)
                        });
        currentCoroutine = StartCoroutine(write);
    }

    public void HandleDownload(PayData payData) {
        if (currentCoroutine != null) {
            StopCoroutine(currentCoroutine);
        }
        IEnumerator write = WriteMany(new Writeln[]{
                            new Writeln("bash>", $"download {payData.filename}", defaultColor, playerType:true)
                        });
        currentCoroutine = StartCoroutine(write);
    }
    public void HandleUtility() {
        if (currentCoroutine != null) {
            StopCoroutine(currentCoroutine);
        }
        IEnumerator write = WriteMany(new Writeln[]{
                            new Writeln("bash>", $"utility.bat --disable --force", defaultColor, playerType:true)
                        });
        currentCoroutine = StartCoroutine(write);
    }
}


public class Writeln {
    public string prefix;
    public string content;
    public bool playerType;
    public float destroyAfter = 0f;
    public Color color;
    public bool flash;
    public Writeln(string prefix, string content, Color color, bool playerType = false) {
        this.prefix = prefix;
        this.content = content;
        this.playerType = playerType;
        this.color = color;
    }
}