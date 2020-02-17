using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class MessageManager : MonoBehaviour
{
    static MessageManager instance = null;
    Stack<Message> myQ;
    DispatchableComponent[] dispatchableComponents;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);return;
        }
        instance = this;
        myQ = new Stack<Message>();
    }

    private void Start()
    {
        dispatchableComponents = FindObjectsOfType<DispatchableComponent>();
    }

    public static MessageManager get()
    {
        return instance;
    }

    void Update()
    {
        DispatchMessage();
    }

    public void SendMessage(Message m)
    {
        if (m.receiver.GetComponent(m.senderComp) == null) return;
        myQ.Push(m);
    }

    public void SendMessageToAll(Message m)
    {
        DispatchableComponent[] receiverCmpnts = dispatchableComponents.Where(c => c.GetType() == m.senderComp).ToArray();
        
        foreach (DispatchableComponent disCmpnt in receiverCmpnts)
        {
            m.receiver = disCmpnt.transform;
            myQ.Push(m);
        }
    }

    public void DispatchMessage()
    {
        foreach(Message m in myQ)
        {
            ((DispatchableComponent) m.receiver.GetComponent(m.senderComp)).Dispatch(m);
        }
        myQ.Clear();
    }
}
