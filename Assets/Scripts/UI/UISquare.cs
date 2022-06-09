using UnityEngine;
using UnityEngine.UI;

public class UISquare : Graphic {
    [SerializeField] float thickness = 1f;

    protected override void OnPopulateMesh(VertexHelper vh) {
        vh.Clear();

        float width = rectTransform.rect.width;
        float height = rectTransform.rect.height;
        Vector2 pivot = new Vector3(rectTransform.pivot.x * width, rectTransform.pivot.y * height);

        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = color;

        vertex.position = new Vector3(0 - pivot.x, 0 - pivot.y);
        vh.AddVert(vertex);
        vertex.position = new Vector3(0 - pivot.x, height - pivot.y);
        vh.AddVert(vertex);
        vertex.position = new Vector3(width - pivot.x, height - pivot.y);
        vh.AddVert(vertex);
        vertex.position = new Vector3(width - pivot.x, 0 - pivot.y);
        vh.AddVert(vertex);

        float widthSqr = thickness * thickness;
        float distanceSqr = widthSqr / 2f;
        float distance = Mathf.Sqrt(distanceSqr);

        vertex.position = new Vector3(distance - pivot.x, distance - pivot.y);
        vh.AddVert(vertex);
        vertex.position = new Vector3(distance - pivot.x, height - distance - pivot.y);
        vh.AddVert(vertex);
        vertex.position = new Vector3(width - distance - pivot.x, height - distance - pivot.y);
        vh.AddVert(vertex);
        vertex.position = new Vector3(width - distance - pivot.x, distance - pivot.y);
        vh.AddVert(vertex);

        // left edge
        vh.AddTriangle(0, 1, 5);
        vh.AddTriangle(5, 4, 0);

        // top edge
        vh.AddTriangle(1, 2, 6);
        vh.AddTriangle(6, 5, 1);

        // right edge
        vh.AddTriangle(2, 3, 7);
        vh.AddTriangle(7, 6, 2);

        // bottom edge
        vh.AddTriangle(3, 0, 4);
        vh.AddTriangle(4, 7, 3);
    }
}
