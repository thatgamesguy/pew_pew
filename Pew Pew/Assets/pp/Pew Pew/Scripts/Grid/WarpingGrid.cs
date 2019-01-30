using UnityEngine;
using System.Collections.Generic;
using GameCore;

namespace WarpGrid
{
    /// <summary>
    /// Updates and displays warping grid.
    /// This is a conversion of an XNA project found here: https://gamedevelopment.tutsplus.com/tutorials/make-a-neon-vector-shooter-in-xna-the-warping-grid--gamedev-9904
    /// </summary>
    public class WarpingGrid : Singleton<WarpingGrid>
    {
        /// <summary>
        /// The grid size. The grid is computationally expensive so make sure you create the smallest grid possible.
        /// </summary>
        public Rect size;

        /// <summary>
        /// The spacing between each line.
        /// </summary>
        public Vector2 spacing;

        /// <summary>
        /// The minimum width of the line. This width is used in the inner squares.
        /// </summary>
        public float minLineWidth = 0.01f;

        /// <summary>
        /// The width of the max line. These lines are used in the outter squares.
        /// </summary>
        public float maxLineWidth = 0.03f;

        /// <summary>
        /// Limits the maximum number of lines in the grid. If this number is too low, the grid will not be drawn in its entirety.
        /// </summary>
        public int maxInstantiatedLines = 1520;

        /// <summary>
        /// The prefab used for the line.
        /// </summary>
        public GameObject linePrefab;

        /// <summary>
        /// The colour of the instantiated lines.
        /// </summary>
        public Color gridColour;

        /// <summary>
        /// Smooth method creates smoother curves but is more expensive. The DrawMethod::Quick is recommended for mobile devices.
        /// </summary>
        public enum DrawMethod { Quick, Smooth };

        /// <summary>
        /// The selected draw method.
        /// </summary>
        public DrawMethod drawMethod = DrawMethod.Quick;

        private static PauseHandler PAUSE_STATUS;

        private Spring[] springs;
        private PointMass[,] points;
        private SpriteRenderer[] m_Renderers;
        private int m_LineIndex = 0;
        private bool initialised = false;

        private void Awake()
        {
            if (!PAUSE_STATUS)
            {
                PAUSE_STATUS = GameObject.FindObjectOfType<PauseHandler>();
            }
        }

        /// <summary>
        /// Creates the grid. Grid is built offscreen and then moved onto the screen to prevent instantiated unpositioned lines from showing onscreen.
        /// </summary>
        public void CreateGrid()
        {
            // Build grid offscreen.
            transform.position -= (Vector3)Vector2.left * 10f;

            var springList = new List<Spring>();

            int numColumns = (int)(size.width / spacing.x) + 1;
            int numRows = (int)(size.height / spacing.y) + 1;
            points = new PointMass[numColumns, numRows];

            // These fixed points will be used to anchor the grid to fixed positions on the screen
            PointMass[,] fixedPoints = new PointMass[numColumns, numRows];

            // cCeate the point masses
            int column = 0, row = 0;

            for (float y = size.yMin; y <= size.yMax; y += spacing.y)
            {
                for (float x = size.xMin; x <= size.xMax; x += spacing.x)
                {
                    points[column, row] = new PointMass(new Vector3(x, y, 0), 1);
                    fixedPoints[column, row] = new PointMass(new Vector3(x, y, 0), 0);
                    column++;
                }
                row++;
                column = 0;
            }

            // Link the point masses with springs
            for (int y = 0; y < numRows; y++)
            {
                for (int x = 0; x < numColumns; x++)
                {
                    if (x == 0 || y == 0 || x == numColumns - 1 || y == numRows - 1)
                    {    // anchor the border of the grid 
                        springList.Add(new Spring(fixedPoints[x, y], points[x, y], 0.1f, 0.1f));
                    }
                    else if (x % 3 == 0 && y % 3 == 0)
                    {                                  // loosely anchor 1/9th of the point masses 
                        springList.Add(new Spring(fixedPoints[x, y], points[x, y], 0.002f, 0.02f));
                    }

                    const float stiffness = 0.28f;
                    const float damping = 0.06f;
                    if (x > 0)
                    {
                        springList.Add(new Spring(points[x - 1, y], points[x, y], stiffness, damping));
                    }

                    if (y > 0)
                    {
                        springList.Add(new Spring(points[x, y - 1], points[x, y], stiffness, damping));
                    }
                }
            }

            if (m_Renderers != null && m_Renderers.Length > 0)
            {
                for (int i = 0; i < m_Renderers.Length; i++)
                {
                    if (m_Renderers[i] != null)
                    {
                        Destroy(m_Renderers[i].gameObject);
                    }
                }
            }

            m_Renderers = new SpriteRenderer[maxInstantiatedLines];
            for (int i = 0; i < maxInstantiatedLines; i++)
            {
                m_Renderers[i] = GetNewLineRenderer();
            }

            springs = springList.ToArray();

            // Move grid back to correct position.
            transform.position += (Vector3)Vector2.left * 10f;

            initialised = true;

            ApplyImplosiveForce(.2f, Vector2.zero, 5f);

        }

        /// <summary>
        /// Destroys grid if created.
        /// </summary>
        public void DisableGrid()
        {
            if (initialised)
            {
                for (int i = 0; i < m_Renderers.Length; i++)
                {
                    Destroy(m_Renderers[i].gameObject);
                }
            }

            initialised = false;
        }

        /// <summary>
        /// Applies a directed force at position.
        /// </summary>
        /// <param name="force">Force.</param>
        /// <param name="position">Position.</param>
        /// <param name="radius">Radius.</param>
        public void ApplyDirectedForce(Vector2 force, Vector2 position, float radius)
        {
            if (!initialised || (PAUSE_STATUS != null && PAUSE_STATUS.isPaused)) { return; }

            foreach (var mass in points)
            {
                var dist = Vector2.Distance(position, mass.Position);
                if (dist < radius)
                {
                    mass.ApplyForce(10 * force / (10 + dist));
                }
            }
        }

        /// <summary>
        /// Applies an implosive force at position.
        /// </summary>
        /// <param name="force">Force.</param>
        /// <param name="position">Position.</param>
        /// <param name="radius">Radius.</param>
        public void ApplyImplosiveForce(float force, Vector2 position, float radius)
        {
            if (!initialised || (PAUSE_STATUS != null && PAUSE_STATUS.isPaused)) { return; }

            foreach (var mass in points)
            {
                float dist2 = Vector2.Distance(position, mass.Position);
                if (dist2 < radius)
                {
                    mass.ApplyForce(10 * force * (position - mass.Position) / (100 + dist2));
                    mass.IncreaseDamping(0.6f);
                }
            }
        }

        /// <summary>
        /// Applies an explosive force at position.
        /// </summary>
        /// <param name="force">Force.</param>
        /// <param name="position">Position.</param>
        /// <param name="radius">Radius.</param>
        public void ApplyExplosiveForce(float force, Vector2 position, float radius)
        {
            if (!initialised || (PAUSE_STATUS != null && PAUSE_STATUS.isPaused)) { return; }

            foreach (var mass in points)
            {
                float dist2 = Vector2.Distance(position, mass.Position);

                if (dist2 < radius)
                {
                    mass.ApplyForce(100 * force * (mass.Position - position) / (10000 + dist2));
                    mass.IncreaseDamping(0.6f);
                }
            }
        }

        void Update()
        {
            if (!initialised || (PAUSE_STATUS != null && PAUSE_STATUS.isPaused)) { return; }

            foreach (var spring in springs)
            {
                spring.Update();
            }

            foreach (var mass in points)
            {
                mass.Update();
            }

            Draw();
        }

        private SpriteRenderer GetNewLineRenderer()
        {
            GameObject line = (GameObject)Instantiate(linePrefab);
            line.transform.SetParent(transform);
            return line.GetComponent<SpriteRenderer>();
        }

        private void Draw()
        {
            if (drawMethod == DrawMethod.Quick)
            {
                QuickDraw();
            }
            else if (drawMethod == DrawMethod.Smooth)
            {
                SmoothDraw();
            }
        }

        private void SmoothDraw()
        {
            int width = points.GetLength(0);
            int height = points.GetLength(1);

            for (int y = 1; y < height; y++)
            {
                for (int x = 1; x < width; x++)
                {
                    Vector2 left = new Vector2(), up = new Vector2();
                    Vector2 p = ToVec2(points[x, y].Position);
                    if (x > 1)
                    {
                        left = ToVec2(points[x - 1, y].Position);
                        float thickness = y % 3 == 1 ? maxLineWidth : minLineWidth;

                        int clampedX = Mathf.Min(x + 1, width - 1);
                        Vector2 mid = Interpolate.CatmullRom(ToVec2(points[x - 2, y].Position), left, p, ToVec2(points[clampedX, y].Position), 0.5f, 05f);

                        var dist = Vector2.Distance(mid, (left + p) / 2);
                        if (dist * dist > 1)
                        {
                            DrawLine(left, mid, gridColour, thickness, m_Renderers[m_LineIndex]);
                            m_LineIndex = (m_LineIndex + 1) % m_Renderers.Length;

                            DrawLine(mid, p, gridColour, thickness, m_Renderers[m_LineIndex]);
                            m_LineIndex = (m_LineIndex + 1) % m_Renderers.Length;
                        }
                        else
                        {
                            DrawLine(left, p, gridColour, thickness, m_Renderers[m_LineIndex]);
                            m_LineIndex = (m_LineIndex + 1) % m_Renderers.Length;
                        }
                    }
                    if (y > 1)
                    {
                        up = ToVec2(points[x, y - 1].Position);
                        float thickness = x % 3 == 1 ? maxLineWidth : minLineWidth;
                        int clampedY = Mathf.Min(y + 1, height - 1);
                        Vector2 mid = Interpolate.CatmullRom(ToVec2(points[x, y - 2].Position), up, p, ToVec2(points[x, clampedY].Position), 0.5f, 0.5f);

                        var dist = Vector2.Distance(mid, (up + p) / 2);
                        if (dist * dist > 1)
                        {
                            DrawLine(up, mid, gridColour, thickness, m_Renderers[m_LineIndex]);
                            m_LineIndex = (m_LineIndex + 1) % m_Renderers.Length;

                            DrawLine(mid, p, gridColour, thickness, m_Renderers[m_LineIndex]);
                            m_LineIndex = (m_LineIndex + 1) % m_Renderers.Length;
                        }
                        else
                        {
                            DrawLine(up, p, gridColour, thickness, m_Renderers[m_LineIndex]);
                            m_LineIndex = (m_LineIndex + 1) % m_Renderers.Length;
                        }
                    }

                    if (x > 1 && y > 1)
                    {
                        Vector2 upLeft = ToVec2(points[x - 1, y - 1].Position);
                        DrawLine(0.5f * (upLeft + up), 0.5f * (left + p), gridColour, minLineWidth, m_Renderers[m_LineIndex]);   // vertical line
                        m_LineIndex = (m_LineIndex + 1) % m_Renderers.Length;

                        DrawLine(0.5f * (upLeft + left), 0.5f * (up + p), gridColour, minLineWidth, m_Renderers[m_LineIndex]);   // horizontal line
                        m_LineIndex = (m_LineIndex + 1) % m_Renderers.Length;
                    }
                }
            }
        }

        private void QuickDraw()
        {
            int width = points.GetLength(0);
            int height = points.GetLength(1);

            for (int y = 1; y < height; y++)
            {
                for (int x = 1; x < width; x++)
                {
                    Vector2 left = new Vector2(), up = new Vector2();

                    Vector2 p = ToVec2(points[x, y].Position);

                    if (x > 1)
                    {
                        left = ToVec2(points[x - 1, y].Position);
                        float thickness = y % 3 == 1 ? maxLineWidth : minLineWidth;


                        DrawLine(left, p, gridColour, thickness, m_Renderers[m_LineIndex]);
                        m_LineIndex = (m_LineIndex + 1) % m_Renderers.Length;
                    }
                    if (y > 1)
                    {
                        up = ToVec2(points[x, y - 1].Position);
                        float thickness = x % 3 == 1 ? maxLineWidth : minLineWidth;

                        DrawLine(up, p, gridColour, thickness, m_Renderers[m_LineIndex]);
                        m_LineIndex = (m_LineIndex + 1) % m_Renderers.Length;
                    }

                    if (x > 1 && y > 1)
                    {
                        Vector2 upLeft = ToVec2(points[x - 1, y - 1].Position);

                        DrawLine(0.5f * (upLeft + up), 0.5f * (left + p), gridColour, minLineWidth, m_Renderers[m_LineIndex]);   // vertical line
                        m_LineIndex = (m_LineIndex + 1) % m_Renderers.Length;

                        DrawLine(0.5f * (upLeft + left), 0.5f * (up + p), gridColour, minLineWidth, m_Renderers[m_LineIndex]);   // horizontal line
                        m_LineIndex = (m_LineIndex + 1) % m_Renderers.Length;

                    }
                }
            }
        }

        private void DrawLine(Vector2 start, Vector2 end, Color color, float thickness, SpriteRenderer renderer)
        {
            var heading = end - start;
            var distance = heading.magnitude;
            var direction = heading / distance;

            Vector3 centerPos = new Vector3(start.x + end.x, start.y + end.y) / 2;
            renderer.transform.position = centerPos;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            renderer.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            var objectWidthSize = 10f / 5f; // 10 = pixels of line sprite, 5f = pixels per units of line sprite.
            renderer.transform.localScale = new Vector3(distance / objectWidthSize, thickness, renderer.transform.localScale.z);

            renderer.color = color;
        }


        private Vector2 ToVec2(Vector3 v)
        {
            // Do a perspective projection
            float factor = (v.z + 2000) / 2000;

            var screenSize = new Vector2(Screen.width, Screen.height);

            return (new Vector2(v.x, v.y) - screenSize / 2f) * factor + screenSize / 2;
        }
    }
}