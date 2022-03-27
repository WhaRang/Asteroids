using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileRenderer : MonoBehaviour
{
    [Header("Data")]
    public Sprite sprite;
    public Mesh mesh;
    public Material material;
    public float life;
    public float speed;
    public float damage;

    [Header("Instances")]
    public List<ProjectileData> projectiles = new List<ProjectileData>();

    //Working values
    private RaycastHit[] rayHitBuffer = new RaycastHit[1];
    private Vector3 worldPoint;
    private Vector3 transPoint;
    private List<Matrix4x4[]> bufferedData = new List<Matrix4x4[]>();

    public void SpawnProjectile(Vector3 position, Quaternion rotation)
    {
        ProjectileData n = new ProjectileData();
        n.pos = position;
        n.rot = rotation;
        n.scale = Vector3.one;
        n.experation = life;

        projectiles.Add(n);
    }

    private void Start()
    {
        SetupMesh();
    }

    private void Update()
    {
        UpdateProjectiles(Time.deltaTime);
        BatchAndRender();
    }

    private void SetupMesh()
    {
        mesh = new Mesh();
        mesh.vertices = System.Array.ConvertAll(sprite.vertices, i => (Vector3)i);
        mesh.uv = sprite.uv;
        mesh.triangles = System.Array.ConvertAll(sprite.triangles, i => (int)i);
    }

    private void BatchAndRender()
    {
        //If we dont have projectiles to render then just get out
        if (projectiles.Count <= 0)
            return;

        //Clear the batch buffer
        bufferedData.Clear();

        //If we can fit all in 1 batch then do so
        if (projectiles.Count < 1023)
            bufferedData.Add(projectiles.Select(p => p.renderData).ToArray());
        else
        {
            //We need multiple batches
            int count = projectiles.Count;
            for (int i = 0; i < count; i += 1023)
            {
                if (i + 1023 < count)
                {
                    Matrix4x4[] tBuffer = new Matrix4x4[1023];
                    for (int ii = 0; ii < 1023; ii++)
                    {
                        tBuffer[ii] = projectiles[i + ii].renderData;
                    }
                    bufferedData.Add(tBuffer);
                }
                else
                {
                    //last batch
                    Matrix4x4[] tBuffer = new Matrix4x4[count - i];
                    for (int ii = 0; ii < count - i; ii++)
                    {
                        tBuffer[ii] = projectiles[i + ii].renderData;
                    }
                    bufferedData.Add(tBuffer);
                }
            }
        }

        //Draw each batch
        foreach (var batch in bufferedData)
            Graphics.DrawMeshInstanced(mesh, 0, material, batch, batch.Length);
    }

    private void UpdateProjectiles(float tick)
    {
        foreach (var projectile in projectiles)
        {
            projectile.experation -= tick;

            if (projectile.experation > 0)
            {
                //Sort out the projectiles 'forward' direction
                transPoint = projectile.rot * Vector3.left;
                //See if its going to hit something and if so handle that
                if (Physics.RaycastNonAlloc(projectile.pos, transPoint, rayHitBuffer, speed * tick) > 0)
                {
                    projectile.experation = -1;
                    worldPoint = rayHitBuffer[0].point;
                    SpawnSplash(worldPoint);
                }
                else
                {
                    //This project wont be hitting anything this tick so just move it forward
                    projectile.pos += transPoint * (speed * tick);
                }
            }
        }
        //Remove all the projectiles that have hit there experation, can happen due to time or impact
        projectiles.RemoveAll(p => p.experation <= 0);
    }

    private void SpawnSplash(Vector3 worlPoint)
    {
        Debug.Log("Splash");
    }
}

[System.Serializable]
public class ProjectileData
{
    public Vector3 pos;
    public Quaternion rot;
    public Vector3 scale;
    public float experation;

    public Matrix4x4 renderData
    {
        get
        {
            return Matrix4x4.TRS(pos, rot, scale);
        }
    }
}