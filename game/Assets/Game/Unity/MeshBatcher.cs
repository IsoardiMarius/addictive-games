using Game.Core.Engine;
using UnityEngine;
using UnityEngine.Rendering;

namespace Game.Unity
{
    /// <summary>
    /// Affiche toute l'image en un seul appel de dessin : la liste de rectangles du jeu est copiée
    /// telle quelle dans un maillage dynamique, texturé par l'atlas généré au démarrage.
    /// </summary>
    public sealed class MeshBatcher
    {
        private static readonly VertexAttributeDescriptor[] Layout =
        {
            new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.UNorm8, 4),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2),
        };

        private const MeshUpdateFlags Fast = MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices
                                             | MeshUpdateFlags.DontNotifyMeshUsers | MeshUpdateFlags.DontResetBoneBounds;

        private static readonly Bounds Huge = new Bounds(Vector3.zero, new Vector3(1e6f, 1e6f, 1e6f));

        private readonly Mesh _mesh;
        private readonly Material _material;
        private readonly Texture2D _texture;
        private int _vertexCapacity;

        public MeshBatcher(GameObject parent, Atlas atlas)
        {
            _texture = new Texture2D(atlas.Width, atlas.Height, TextureFormat.RGBA32, false, false);
            _texture.name = "DigForReal.Atlas";
            _texture.filterMode = FilterMode.Bilinear;
            _texture.wrapMode = TextureWrapMode.Clamp;
            _texture.anisoLevel = 0;
            _texture.LoadRawTextureData(atlas.Pixels);
            _texture.Apply(false, true);

            Shader shader = Resources.Load<Shader>("Shaders/SpriteBatch");
            if (shader == null)
            {
                Debug.LogError("Dig For Real : shader introuvable (Assets/Game/Resources/Shaders/SpriteBatch.shader).");
                shader = Shader.Find("Sprites/Default");
            }
            _material = new Material(shader);
            _material.mainTexture = _texture;

            var go = new GameObject("Batch");
            go.transform.SetParent(parent.transform, false);
            // Le jeu compte y vers le bas ; Unity vers le haut.
            go.transform.localScale = new Vector3(1f, -1f, 1f);
            _mesh = new Mesh();
            _mesh.name = "DigForReal.Batch";
            _mesh.MarkDynamic();
            go.AddComponent<MeshFilter>().sharedMesh = _mesh;
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = _material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
        }

        private void Grow(int vertexCount)
        {
            int capacity = Mathf.Max(4096, Mathf.NextPowerOfTwo(vertexCount));
            _vertexCapacity = capacity;
            _mesh.Clear();
            _mesh.SetVertexBufferParams(capacity, Layout);
            int quads = capacity / 4;
            int[] indices = new int[quads * 6];
            for (int q = 0; q < quads; q++)
            {
                int v = q * 4, i = q * 6;
                indices[i] = v; indices[i + 1] = v + 1; indices[i + 2] = v + 2;
                indices[i + 3] = v; indices[i + 4] = v + 2; indices[i + 5] = v + 3;
            }
            _mesh.SetIndexBufferParams(indices.Length, IndexFormat.UInt32);
            _mesh.SetIndexBufferData(indices, 0, 0, indices.Length, Fast);
            _mesh.bounds = Huge;
        }

        public void Upload(DrawList list)
        {
            int count = list.Count;
            if (count > _vertexCapacity) Grow(count);
            if (count > 0) _mesh.SetVertexBufferData(list.Verts, 0, 0, count, 0, Fast);
            // Les bornes n'étant pas recalculées (gain de temps), la plage de sommets utilisée est indiquée explicitement.
            var range = new SubMeshDescriptor(0, count / 4 * 6, MeshTopology.Triangles)
            {
                baseVertex = 0,
                firstVertex = 0,
                vertexCount = count,
                bounds = Huge
            };
            _mesh.subMeshCount = 1;
            _mesh.SetSubMesh(0, range, Fast);
            _mesh.bounds = Huge;
        }
    }
}
