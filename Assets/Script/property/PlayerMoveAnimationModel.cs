using UnityEngine;
using System.Collections;
using System;

public class PlayerMoveAnimationModel {

    public bool IsMove;
    //自定义时间类型,因为json在把时间转换成字符串时,会丢失后面的毫秒数,导致精度丢失,动画无法更新

    public string time;
    public void SetTime(DateTime dateTime)
    {
        time = dateTime.ToString("yyyyMMddHHmmssffff");
    }

    public DateTime GetTime()
    {
        DateTime dt = DateTime.ParseExact(time, "yyyyMMddHHmmssffff", System.Globalization.CultureInfo.CurrentCulture);
        return dt;
    }
}
