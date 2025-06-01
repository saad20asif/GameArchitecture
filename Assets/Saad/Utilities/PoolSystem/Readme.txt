# PoolManagerSO - Unity Pooling System

This is a flexible, generic pooling system using ScriptableObject (`PoolManagerSO`) that allows pooling of both:
- GameObjects (e.g., UI panels, VFX)
- Components (e.g., specific MonoBehaviours like `MyView`, `EnemyController`)

It uses Unity's built-in `ObjectPool<T>` under the hood and supports:
✔️ Prewarming  
✔️ Capacity/size limits  
✔️ GameObject or Component pooling  
✔️ IPoolable callbacks  
✔️ Scene-safe behavior

---

## 🔧 SETUP

### 1. Create PoolManager
- Right-click in Project → `Create → Pools → Pool Manager`
- Name it: `UIStatesPoolManager` or `GameplayPoolManager`, etc.

### 2. Add Pools
Inside the `PoolManagerSO`, configure `PoolConfigs[]`:

- `PoolID`: A unique key string (e.g. `"MainMenu"`)
- `Prefab`: The GameObject you want to pool
- `DefaultCapacity`: How many to create at start
- `MaxSize`: The max number allowed in the pool
- `Prewarm`: Whether to instantiate DefaultCapacity at startup

---

## 🧠 IPoolable Interface (Optional)

Your pooled GameObjects can implement `IPoolable` to get lifecycle callbacks:

```csharp
public interface IPoolable
{
    void OnPoolGet();      // Called when object is retrieved from pool
    void OnPoolRelease();  // Called when object is returned to pool
}
