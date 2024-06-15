using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using UnityEngine;
public class ObjectiveCanvasController : MonoBehaviour {
    public TerminalAnimation terminalAnimation;
    public Color successColor;
    public Color failColor;
    public Color inProgressColor;


    public void Initialize(GameData gameData) {
        Bind();
    }

    public void Bind() {
        GameManager.OnObjectivesChange += HandleObjectivesChange;
        terminalAnimation.Clear();
    }
    void OnDestroy() {
        GameManager.OnObjectivesChange -= HandleObjectivesChange;
    }
    public void HandleObjectivesChange(List<ObjectiveDelta> allObjectives, ObjectiveDelta changedStatuses, bool optional) {
        if (changedStatuses != null) {
            Writeln[] writes = allObjectives.Select(delta => {
                Color color = delta.status switch {
                    ObjectiveStatus.inProgress => inProgressColor,
                    ObjectiveStatus.complete => successColor,
                    ObjectiveStatus.failed => failColor,
                    _ => inProgressColor
                };
                bool doFlash = changedStatuses == delta;
                string title = optional ? $"[optional] {delta.template.title}" : delta.template.title;
                return new Writeln("", $"{title}:\t\t{delta.status}", color) {
                    destroyAfter = 5f,
                    flash = doFlash
                };
            }).ToArray();

            terminalAnimation.DoWriteMany(writes);
        }
    }

}
