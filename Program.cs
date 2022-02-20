using System;
using System.Collections.Generic;
using System.Numerics;

namespace BSP
{
    // TODO:
    // Переделать методы с List<int> на List<Edge>
    class Program
    {
        static void Main(string[] args)
        {
            List<int> list = new List<int> {0, 0, 0, 5, 5, 5, 5, 0};
            Rectangle R = new Rectangle(list);

            Console.WriteLine($"Длина {R.Width}, высота {R.Height}");
            for (int i = 0; i < R.Edges.Count; i++)
            {
                Console.WriteLine($"Отрезок с точками ({R.Edges[i].Left.x}, {R.Edges[i].Left.y}), ({R.Edges[i].Right.x}, {R.Edges[i].Right.y}) ");
            }
        }

    }

    class Scene
    {
        public Node Root { get; set; }
        public void BuildScene (List<Edge> Edges)
        {
            if (this.Root == null)
            {
                
            }
        }
    }

    class Node
    {
        public List<Edge> Edges { get; set; }
        public Rectangle Space { get; set; }
        public Node LeftNode { get; set; }
        public Node RightNode { get; set; }
        public Edge Segment { get; set; }

        public bool Horizontal = false;

        public void FindSegment()
        {
            if (Edges != null && Space != null)
            {
                Edge seg = Edges[0];

                if (seg.Left.x == seg.Right.x)
                {
                    // Vertical
                    this.Segment = seg.CreateHorizontalEdge(seg.Left.x, Space.yl, Space.yr);
                }
                else if (seg.Left.y == seg.Right.y)
                {
                    // Horizontal
                    this.Segment = seg.CreateHorizontalEdge(seg.Left.y, Space.xl, Space.xr);
                    this.Horizontal = true;
                }
            }
        }

        /// <summary>
        /// Определяет положение точки относительно Segment секущей
        /// </summary>
        /// <param name="edge">Грань</param>
        /// <returns>
        /// 0: COINCIDENT
        /// 1: LEFT/DOWN
        /// 2: RIGHT/TOP
        /// 3: SPANNING
        /// </returns>
        public int ClassifyEdge(Edge edge)
        {
            int result = 0;
            if(Segment != null)
            {
                if(Horizontal)
                {
                    bool top1 = edge.Left.y > Segment.Left.y; // Первая точка выше
                    bool top2 = edge.Right.y > Segment.Left.y; // Вторая точка выше
                    if (!top1 && !top2)
                    {
                        result = 1;
                    }
                    else if(top1 && top2)
                    {
                        result = 2;
                    }
                    else if((top1 && !top2) || (!top1 && top2))
                    {
                        result = 3;
                    }
                }
                else
                {
                    bool right1 = edge.Left.x > Segment.Left.x; // Первая точка выше
                    bool right2 = edge.Right.x > Segment.Left.x; // Вторая точка выше
                    if (!right1 && !right2)
                    {
                        result = 1;
                    }
                    else if (right1 && right2)
                    {
                        result = 2;
                    }
                    else if ((right1 && !right2) || (!right1 && right2))
                    {
                        result = 3;
                    }
                }
            }
            return result;
        }

    }

    class Rectangle
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int xl { get; set; }
        public int yl { get; set; }
        public int xr { get; set; }
        public int yr { get; set; }

        public List<Edge> Edges;

        public Rectangle() { }

        /// <summary>
        /// Пространство задается 4 вершинами - 8 координатами
        /// Объявляются 4 грани области Rectangle
        /// </summary>
        /// <param name="array">
        /// Массив координат x, y начиная с левой нижней по часовой стрелке
        /// </param>
        public Rectangle(List<int> array)
        {
            this.xl = array[0];
            this.yl = array[1];
            this.xr = array[4];
            this.yr = array[5];
            this.SetWidth(xr, xl);
            this.SetHeight(yr, yl);

            List<int> line1 = new List<int> {array[0], array[1], array[2], array[3] };
            Edge edge1 = new Edge(this, line1);

            List<int> line2 = new List<int> { array[0], array[1], array[6], array[7] };
            Edge edge2 = new Edge(this, line2);

            List<int> line3 = new List<int> { array[4], array[5], array[2], array[3] };
            Edge edge3 = new Edge(this, line3);

            List<int> line4 = new List<int> { array[4], array[5], array[6], array[7] };
            Edge edge4 = new Edge(this, line4);


            Edges = new List<Edge>();
            this.Edges.Add(edge1);
            this.Edges.Add(edge2);
            this.Edges.Add(edge3);
            this.Edges.Add(edge4);
        }

        public Rectangle(List<Edge> edges)
        {
            // Реализовать для Edge
        }

        public void SetWidth(int x1, int x2)
        {
            this.Width = Math.Abs(x1 - x2);
        }
        public void SetHeight(int y1, int y2)
        {
            this.Height = Math.Abs(y1 - y2);
        }
    }

    class Edge
    {
        Rectangle Parent { get; set; }
        public Point Left { get; set; }
        public Point Right { get; set; }
        public Edge() { }
        public Edge(Rectangle parent, List<int> array)
        {
            Point left = new Point(array[0], array[1]);
            Point right = new Point(array[2], array[3]);

            this.Left = left;
            this.Right = right;
            this.Parent = parent;
        }

        public Edge(List<int> array)
        {
            Point left = new Point(array[0], array[1]);
            Point right = new Point(array[2], array[3]);

            this.Left = left;
            this.Right = right;
        }

        public Edge CreateHorizontalEdge(int y, int xl, int xr)
        {
            List<int> Vertexs = new List<int> { xl, y, xr, y };
            Edge edge = new Edge(Vertexs);
            return edge;
        }

        public Edge CreateVerticalEdge(int x, int yl, int yr)
        {
            List<int> Vertexs = new List<int> { x, yl, x, yr };
            Edge edge = new Edge(Vertexs);
            return edge;
        }

        /// <summary>
        /// Разрезает Edge
        /// Передается , тип разрезания Горизонтальное/Вертикальное
        /// </summary>
        /// <param name="splitter">Грань-секущая</param>
        /// <param name="horizontal">Тип сечения</param>
        /// <param name="leftEdge">Левый/Нижний отрезок</param>
        /// <param name="rightEdge">Правый/Верхний отрезок</param>
        public void SplitEdge(Edge splitter, bool horizontal, ref Edge leftEdge, ref Edge rightEdge)
        {
            if(horizontal)
            {
                // splitter.Left.y == splitter.Right.y
                leftEdge.Left.y = splitter.Left.y;
                leftEdge.Right.y = splitter.Left.y;
            }
            else
            {
                // splitter.Left.x == splitter.Right.x
                leftEdge.Left.x = splitter.Left.x;
                leftEdge.Right.x = splitter.Left.x;
            }
        }
    }

    class Point
    {
        public int x { get; set; }
        public int y { get; set; }
        public Point() { }
        public Point(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
