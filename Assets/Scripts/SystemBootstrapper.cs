using CentralTech.CTResilientAnalytics;
using CentralTech.SystemsBase;
using UnityEngine;

public class SystemBootstrapper : MonoBehaviour
{
    private static SystemBootstrapper _instance;
    
    private IResilientAnalyticsSystem _resilientAnalyticsSystem;
    //private IResilientAnalyticsSystemV2 _resilientAnalyticsSystemV2;
    void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(this.gameObject);

        _resilientAnalyticsSystem = new ResilientAnalyticsSystem(5f);
       // _resilientAnalyticsSystemV2 = new ResilientAnalyticsSystemV2(5f);
        SystemProvider.Instance.Register(_resilientAnalyticsSystem);
        
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.A))
        {
            _resilientAnalyticsSystem.SendEvent("Blaaa");
        }
       // _resilientAnalyticsSystemV2.Update();
    }

    void OnDestroy()
    {
        if (_instance == this)
        {
            SystemProvider.Instance.Unregister(_resilientAnalyticsSystem);
            _instance = null;
        }
    }

}
