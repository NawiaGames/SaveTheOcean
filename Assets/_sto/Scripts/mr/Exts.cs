using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace mr
{
  public struct Range<T>
  {
    public T beg;
    public T end;

    public Range(T beg_val, T end_val)
    {
      beg = beg_val;
      end = end_val;
    }
  }
}

public static class Vector2IntEx
{
  public static Vector2Int to_units(this Vector2Int v)
  {
    int x = (v.x != 0)? v.x / Mathf.Abs(v.x) : 0;
    int y = (v.y != 0)? v.y / Mathf.Abs(v.y) : 0;

    return new Vector2Int(x, y);
  }
}
public static class Vector3Ex
{
  public static Vector3 bezier(Vector3[] vctrl_pts, float t)
  {
    float tc = Mathf.Clamp01(t);
    float t2 = tc*tc;
    float t3 = t2*tc;
    float rev_t = 1f - tc;
    float rev_t2 = rev_t * rev_t;
    float rev_t3 = rev_t2 * rev_t;
    return rev_t3 * vctrl_pts[0] + 3f * tc * rev_t2 * vctrl_pts[1] + 3f * t2 * rev_t * vctrl_pts[2] + t3 * vctrl_pts[3];
  }
  public static Vector3 swap_yz(this Vector3 vec)
  {
    return new Vector3(vec.x, vec.z, vec.y);
  }
  public static Vector3 swap_xz(this Vector3 vec, float mx = 1.0f, float mz = 1.0f)
  {
    return new Vector3(vec.z * mz, vec.y, vec.x * mx);
  }
  public static Vector2 get_xz(this Vector3 vec)
  {
    return new Vector2(vec.x, vec.z);
  }
  public static Vector3 clamp(this Vector3 vec, Vector3 minLim, Vector3 maxLim)
  {
    return new Vector3(Mathf.Clamp(vec.x, minLim.x, maxLim.x), Mathf.Clamp(vec.y, minLim.y, maxLim.y), Mathf.Clamp(vec.z, minLim.z, maxLim.z));
  }
  // public static void set_x(this Vector3? v, float x)
  // {
  //   var vt = v.Value;
  //   vt.x = x;
  //   v = vt;
  // }
  // public static void set_y(this Vector3? v, float y)
  // {
  //   var vt = v.Value;
  //   vt.y = y;
  //   v = vt;
  // }
  public static void set_z(this Vector3? v, float z)
  {
    var vt = v.Value;
    vt.z = z;
    v = vt;
  }
  public static Vector3 round(this Vector3 v)
  {
    return new Vector3(Mathf.Round(v.x), Mathf.Round(v.y), Mathf.Round(v.z));
  }
}
public static class TrasformEx
{
  public static void reset(this Transform tr)
  {
    tr.localPosition = Vector3.zero;
    tr.localRotation = Quaternion.identity;
    tr.localScale = Vector3.one;
  }
  public static void set_pos_x(this Transform tr, float x)
  {
    tr.position = new Vector3(x, tr.position.y, tr.position.z);
  }
  public static void set_pos_y(this Transform tr, float y)
  {
    tr.position = new Vector3(tr.position.x, y, tr.position.z);
  }
  public static void set_pos_z(this Transform tr, float z)
  {
    tr.position = new Vector3(tr.position.x, tr.position.y, z);
  }
  public static void set_local_pos_x(this Transform tr, float x)
  {
    tr.localPosition = new Vector3(x, tr.localPosition.y, tr.localPosition.z);
  }
  public static void set_local_pos_y(this Transform tr, float y)
  {
    tr.localPosition = new Vector3(tr.localPosition.x, y, tr.localPosition.z);
  }
  public static void set_local_pos_z(this Transform tr, float z)
  {
    tr.localPosition = new Vector3(tr.localPosition.x, tr.localPosition.y, z);
  }
}
public static class CMath
{
  public static float normalize_angle(float ang)
  {
    return ang > 180 ? ang - 360 : ang;
  }
  public static float RoundTo(float value, float multipleOf)
  {
    return Mathf.Round(value / multipleOf) * multipleOf;
  }
  public static int RoundToInt(int value, int multipleOf)
  {
    return Mathf.RoundToInt(value / multipleOf) * multipleOf;
  }
}

public static class StringEx
{
  public static string Reverse(this string str)
  {
    string str_out = str;
    if(!string.IsNullOrEmpty(str))
    {
      char[] charr = str.ToArray();
      System.Array.Reverse(charr);
      str_out = new string(charr);
    }
    return str_out;
  }
}
public static class ListEx
{
  public static void shuffle<T>(this List<T> list, int swap_cnt)
  {
    if(list != null && list.Count > 0)
    {
      for(int q = 0; q < swap_cnt; ++q)
      {
        int i0 = Random.Range(0, list.Count);
        int i1 = Random.Range(0, list.Count);
        T tmp = list[i0];
        list[i0] = list[i1];
        list[i1] = tmp;
      }
    }
  }
  public static T get_random<T>(this List<T> list)
  {
    return list[Random.Range(0, list.Count)];
  }
  public static int get_random_idx<T>(this List<T> list)
  {
    return Random.Range(0,list.Count);
  }
  public static T first<T>(this List<T> list)
  {
    return list[0];
  }
  public static T last<T>(this List<T> list, int idx_from_end = 0)
  {
    return list[list.Count - 1 + idx_from_end];
  }
  public static int last_idx<T>(this List<T> list)
  {
    return list.Count - 1;
  }
  public static List<T> clone<T>(this List<T> list)
  {
    return new List<T>(list);
  }
  public static void swap<T>(this List<T> list, int idx0, int idx1)
  {
    T tmp = list[idx0];
    list[idx1] = list[idx0];
    list[idx0] = tmp;
  }
}
public static class ArrayEx
{
  public static T get_random<T>(this T[] array)
  {
    return array[Random.Range(0, array.Length)];
  }
  public static int get_random_idx<T>(this T[] array)
  {
    return Random.Range(0, array.Length);
  }
  public static T first<T>(this T[] array)
  {
    return array[0];
  }
  public static T last<T>(this T[] array, int idx_from_end = 0)
  {
    return array[array.Length - 1 + idx_from_end];
  }
  public static int last_idx<T>(this T[] array)
  {
    return array.Length-1;
  }
  public static void shuffle<T>(this T[] array, int swap_cnt)
  {
    for(int q = 0; q < swap_cnt; ++q)
    {
      int i0 = Random.Range(0, array.Length);
      int i1 = Random.Range(0, array.Length);
      T tmp = array[i0];
      array[i0] = array[i1];
      array[i1] = tmp;
    }
  }
  public static void swap<T>(this T[] array, int idx0, int idx1)
  {
    T tmp = array[idx0];
    array[idx1] = array[idx0];
    array[idx0] = tmp;
  }
}
public static class TimeEx
{
  ///
  public static float deltaTimeFrame => 60 * Time.deltaTime;
  public static float deltaTimeFrameInv => 1 / deltaTimeFrame;
}
public static class MonoExt
{
  public static void Invoke(this MonoBehaviour mono, System.Action methodToInvoke, float delay)
  {
    mono.StartCoroutine(InvokeSequence(delay, methodToInvoke));
  }
  static IEnumerator InvokeSequence(float delay, System.Action action)
  {
    if(delay >=0)
      yield return new WaitForSeconds(delay);
    else
    {
      for(int q = (int)delay; q < 0; ++q)
        yield return null;
    }
    action();
  }
  public static void Invoke<T>(this MonoBehaviour mono, System.Action<T> methodToInvoke, T invoke_arg, float delay)
  {
    mono.StartCoroutine(InvokeSequence<T>(delay, methodToInvoke, invoke_arg));
  }
  static IEnumerator InvokeSequence<T>(float delay, System.Action<T> action, T invoke_arg)
  {
    if(delay >=0)
      yield return new WaitForSeconds(delay);
    else
    {
      for(int q = (int)delay; q < 0; ++q)
        yield return null;
    }
    action(invoke_arg);
  }
  public static void Invoke<T0, T1>(this MonoBehaviour mono, System.Action<T0, T1> methodToInvoke, (T0, T1) invoke_arg, float delay)
  {
    mono.StartCoroutine(InvokeSequence<T0, T1>(delay, methodToInvoke, invoke_arg));
  }
  static IEnumerator InvokeSequence<T0, T1>(float delay, System.Action<T0, T1> action, (T0, T1) invoke_args)
  {
    if(delay >=0)
      yield return new WaitForSeconds(delay);
    else
    {
      for(int q = (int)delay; q < 0; ++q)
        yield return null;
    }
    action(invoke_args.Item1, invoke_args.Item2);
  }
  static IEnumerator InvokeSequence<T0, T1, T2>(float delay, System.Action<T0, T1, T2> action, (T0, T1, T2) invoke_args)
  {
    if(delay >=0)
      yield return new WaitForSeconds(delay);
    else
    {
      for(int q = (int)delay; q < 0; ++q)
        yield return null;
    }
    action(invoke_args.Item1, invoke_args.Item2, invoke_args.Item3);
  }
}

public static class AnimatorExt
{
  public static IEnumerator WaitForAnimState(this Animator animator, string anim)
  {
    yield return null;
    while(!animator.GetCurrentAnimatorStateInfo(0).IsName(anim))
    {
      yield return null;
    }
    yield return null;
  }
  public static IEnumerator InvokeForAnimState(this Animator animator, string anim, System.Action action)
  {
    yield return null;
    while(!animator.GetCurrentAnimatorStateInfo(0).IsName(anim))
    {
      yield return null;
    }
    yield return null;

    action?.Invoke();
  }
  public static IEnumerator InvokeForAnimState<T>(this Animator animator, string anim, System.Action<T> action, T invoke_args)
  {
    yield return null;
    while(!animator.GetCurrentAnimatorStateInfo(0).IsName(anim))
    {
      yield return null;
    }
    yield return null;

    action?.Invoke(invoke_args);
  }

  public static IEnumerator WaitForAnimStateEnd(this Animator animator, string anim)
  {
    yield return null;
    while(animator.GetCurrentAnimatorStateInfo(0).IsName(anim))
    {
      yield return null;
    }
    yield return null;
  }
  public static IEnumerator InvokeForAnimStateEnd(this Animator animator, string anim, System.Action action)
  {
    yield return null;
    while(animator.GetCurrentAnimatorStateInfo(0).IsName(anim))
    {
      yield return null;
    }
    yield return null;

    action?.Invoke();
  }
  public static IEnumerator InvokeForAnimStateEnd<T>(this Animator animator, string anim, System.Action<T> action, T invoke_args)
  {
    yield return null;
    while(animator.GetCurrentAnimatorStateInfo(0).IsName(anim))
    {
      yield return null;
    }
    yield return null;

    action?.Invoke(invoke_args);
  }
}