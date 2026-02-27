using CentralTech.CTResilientAnalytics;
using CentralTech.CTEventSystem;
using CentralTech.CTSystemsBase;
using UnityEngine;

public class SystemBootstrapper : MonoBehaviour
{
    private static SystemBootstrapper _instance;
    
    private IResilientAnalyticsSystem _resilientAnalyticsSystem;
    private IEventSystem _eventSystem;
    void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(this.gameObject);

        _eventSystem = new EventSystem();
        _resilientAnalyticsSystem = new ResilientAnalyticsSystem(_eventSystem,1f);

        SystemProvider.Instance.Register(_resilientAnalyticsSystem);
        SystemProvider.Instance.Register(_eventSystem);
        
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.A))
        {
            _resilientAnalyticsSystem.SendEvent("Thing" + Random.Range(0, 1000));
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            _resilientAnalyticsSystem.BlockEventSending();
        }
        
        if(Input.GetKeyDown(KeyCode.U))
        {
            _resilientAnalyticsSystem.UnblockEventSending();
        }
    }

    void OnDestroy()
    {
        if (_instance == this)
        {
            SystemProvider.Instance.Unregister(_resilientAnalyticsSystem);
            SystemProvider.Instance.Unregister(_eventSystem);
            _instance = null;
        }
    }

}
