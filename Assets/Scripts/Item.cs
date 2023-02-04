using UnityEngine;

public class Item : MonoBehaviour, IHarvestable
{
    [SerializeField]
    private ItemData data;

    [SerializeField]
    private State state;

    [SerializeField]
    private GameObject underground;

    [SerializeField]
    private GameObject pulled;

    [SerializeField]
    private GameObject hole;

    // Is the Item being harvested?
    // Used to update UI and harvesting state.
    private bool harvesting;

    public enum State
    {
        Available,
        Pulled,
        Empty
    }

    public void OnTriggerEnter(Collider other)
    {
        TryRegisterHarvestable(other);
    }

    public void OnTriggerExit(Collider other)
    {
        TryUnregisterHarvestable(other);
    }

    public void SetState(State state)
    {
        this.state = state;

        switch (this.state)
        {
            case State.Available:
                underground.SetActive(true);
                pulled.SetActive(false);
                hole.SetActive(false);
                break;

            case State.Pulled:
                underground.SetActive(false);
                pulled.SetActive(true);
                hole.SetActive(true);
                break;

            case State.Empty:
                underground.SetActive(false);
                pulled.SetActive(false);
                hole.SetActive(true);
                break;
        }
    }

    public string GetTitle()
    {
        return data.title;
    }

    #region Harvestables

    public ItemData Harvest(bool harvesting)
    {
        this.harvesting = harvesting;
        return data; // For now, harvest immediately, raise event later
    }

    private void TryRegisterHarvestable(Collider other)
    {
        var collector = other.GetComponentInChildren<ICollector>();
        if (collector != null) collector.RegisterHarvestable(this);
    }

    private void TryUnregisterHarvestable(Collider other)
    {
        var collector = other.GetComponentInChildren<ICollector>();
        if (collector != null) collector.UnregisterHarvestable(this);
    }

    #endregion
}
