using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EInterchangableSide
{
    R,
    L
}
public struct InterchangableSlot
{
    public int interchangableIndex;
    public IInterchangable interchangable;
    public EInterchangableSide side;
    public Vector3 Position
    {
        get {
            if (!IsValid())
            {
                Debug.LogError("[InterchangableSlot: Position] Error: No interchangable selected.");
            }
            return interchangable.GetGameObject().transform.position; 
        }
    }
    public InterchangableSlot(EInterchangableSide side){
        interchangableIndex = 0;
        interchangable = null;
        this.side = side;
    }
    public void SelectInterChangable(List<IInterchangable> interchangables )
    {
        if (interchangables.Count == 0)
        {
            return;
        }

        interchangableIndex++;
        if (interchangable == null || interchangableIndex >= interchangables.Count)
        {
            interchangableIndex = 0;
        }

        if (interchangable != null)
        {
            interchangable.Unselect();
        }

        interchangable = GetFirstValidInterchangable(interchangables);

        if (interchangable != null)
        {
            interchangable.Select(side);
        }
    }
    public IInterchangable GetFirstValidInterchangable(List<IInterchangable> interchangables)
    {
        int currentCheckingIndex = interchangableIndex;
        if (!interchangables[currentCheckingIndex].IsSelected())
        {
            return interchangables[currentCheckingIndex];
        }
        currentCheckingIndex++;
        while (currentCheckingIndex != interchangableIndex)
        {
            if (currentCheckingIndex >= interchangables.Count)
            {
                currentCheckingIndex = 0;
            }
            if (!interchangables[currentCheckingIndex].IsSelected())
            {
                interchangableIndex = currentCheckingIndex;
                return interchangables[currentCheckingIndex];
            }
            currentCheckingIndex++;
        }
        return null;
        
    }
    public bool IsValid()
    {
        return interchangable != null;
    }
    public void Interchange(Vector3 newPos)
    {
        if (!IsValid())
        {
            return;
        }
        interchangable.Interchange(newPos);
        interchangable.Unselect();
        interchangable = null;
    }
    
}
public class Interchanger : MonoBehaviour
{
    [SerializeField]
    private PlayerInput playerInput;
    private List<IInterchangable> interchangables = new List<IInterchangable>();
    private InterchangableSlot interchangableL;
    private InterchangableSlot interchangableR;

    void Start()
    {
        GetInterchangables();
        interchangableL = new InterchangableSlot(EInterchangableSide.L);
        interchangableR = new InterchangableSlot(EInterchangableSide.R);
    }
    private void OnEnable()
    {
        playerInput.OnSelectEvent += SelectInterChangable;
        playerInput.OnInterchangeEvent += Interchange;
    }
    private void OnDisable()
    {
        playerInput.OnSelectEvent -= SelectInterChangable;
        playerInput.OnInterchangeEvent -= Interchange;
    }
    private void GetInterchangables()
    {
        GameObject[] interchangablesGO = GameObject.FindGameObjectsWithTag("Interchangable");
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        interchangables = new List<IInterchangable>();
        if (playerGO.TryGetComponent<IInterchangable>(out IInterchangable interchangablePlayer))
        {
            interchangables.Add(interchangablePlayer);
        }
        for (int i = 0; i < interchangablesGO.Length; i++)
        {

            if (interchangablesGO[i].TryGetComponent<IInterchangable>(out IInterchangable interchangable))
            {
                interchangables.Add(interchangable);
            }
        }
    }
    void SelectInterChangable(EInterchangableSide side)
    {
        if (side == EInterchangableSide.R)
        {
            interchangableL.SelectInterChangable(interchangables);
            return;
        }
        interchangableR.SelectInterChangable(interchangables);
    }
    private void Interchange()
    {
        if (!interchangableR.IsValid() || !interchangableL.IsValid())
        {
            return;
        }
        Vector3 auxMovablePosition = interchangableL.Position;
        interchangableL.Interchange(interchangableR.Position);
        interchangableR.Interchange(auxMovablePosition);
    }
}
