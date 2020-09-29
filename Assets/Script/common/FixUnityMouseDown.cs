using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public class FixUnityMouseDown : MonoBehaviour {
    void Update()
    {
        if (Input.GetMouseButtonDown (0)) //按下鼠标左键
        {
            //摄像机到鼠标点击处的射线
            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            //射线击中的物体
            List<RaycastHit> orderedHits = new List<RaycastHit>(Physics.RaycastAll(mouseRay));
            //给射线击中的物体排序
            orderedHits.Sort ((h1,h2) => h1.distance.CompareTo (h2.distance));
            bool hasBeenConsumed = false;
            foreach (RaycastHit hit in orderedHits)
            {
                if (hasBeenConsumed)
                {
                   break;
                }
   
                ComponentWithMethod target = ComponentThatCanReceiveMethod(hit.collider.gameObject, "FixedOnMouseDown");
                if (target.component != null)
                {
                    //成功-返回包含返回值的object, 失败-返回null
                     object result = target.method.Invoke(target.component, null);
                     bool didConsume = (result == null) ? (true /** assume consumption if not specified */ ) : (bool)result;
                     if (!didConsume) //方法调用失败
                     {
                        continue;
                     }
                    else //方法调用成功
                    {
                        hasBeenConsumed = true;
                        break;
                     }
                }    
            }
        }
}                    
 
    struct ComponentWithMethod
    {
         public Component component;
         public MethodInfo method;
    }

    private ComponentWithMethod ComponentThatCanReceiveMethod(GameObject go, string methodName)
    {
        foreach (Component subComponent in go.GetComponents ( typeof(Component)))
        {
              MethodInfo info = subComponent.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
              if (info != null)
              {
                  ComponentWithMethod result = new ComponentWithMethod();
                  result.component = subComponent;
                  result.method = info;
                  return result;
              }
        }
  
        /**
        didn't find aything on this object or its components.
        So ... check again on parent object. Keep going till you find a match or fail
        */

        if (go.transform.parent != null)
        {
             return ComponentThatCanReceiveMethod(go.transform.parent.gameObject, methodName);
        }
        return new ComponentWithMethod();
    }   
}
