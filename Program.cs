using System;
using System.Collections.Generic;
using System.Numerics;

namespace BSP
{
    // TODO:
    // [X] Переделать методы с List<int> на List<Edge>
    // [ ] Проверить SplitEdge
    // [ ] Сделать проверку у точке выхода. Если все вершины листа принадлежат списку вершин у Root, то такой лист является препядствием
    class Program
    {
        static void Main(string[] args)
        {
            List<int> vertexs;
            vertexs = new List<int> { 0, 0, 0, 5, 0, 5, 5, 5, 5, 5, 5, 0, 5, 0, 0, 0 };
            vertexs = new List<int> {0, 0, 0, 5, 0, 5, 5, 5, 5, 5, 5, 0, 5, 0, 0, 0, 3, 2, 3, 4, 3, 4, 4, 4, 4, 4, 4, 2, 4, 2, 3, 2};
            vertexs = new List<int> { 0, 0, 0, 5, 0, 5, 5, 5, 5, 5, 5, 0, 5, 0, 0, 0, 2, 4, 2, 5, 3, 5, 3, 4 };

            List<Edge> edges = new List<Edge>();
            for (int i = 0; i < vertexs.Count / 4; i++)
            {
                edges.Add(new Edge(vertexs[i * 4], vertexs[i * 4 + 1], vertexs[i * 4 + 2], vertexs[i * 4 + 3]));
            }

            Scene scene = new Scene();
            Node root = new Node(edges);
            scene.BuildTree(ref root, edges);
            scene.GetTree(root);
            // Вывод граней
            /*
            for (int i = 0; i < edges.Count; i++)
            {
                Console.WriteLine($"LEFT: ({edges[i].Left.x}, {edges[i].Left.y}), RIGHT: ({edges[i].Right.x}, {edges[i].Right.y})");
            }
            */

            /*
            Rectangle R = new Rectangle(list);

            Console.WriteLine($"Длина {R.Width}, высота {R.Height}");
            for (int i = 0; i < R.Edges.Count; i++)
            {
                Console.WriteLine($"Отрезок с точками ({R.Edges[i].Left.x}, {R.Edges[i].Left.y}), ({R.Edges[i].Right.x}, {R.Edges[i].Right.y}) ");
            }
            */
        }

    }

    class Scene
    {
        public Node Root { get; set; }
        public void GetTree(Node node, string indent = "")
        {
            if (node != null)
            {
                indent += new string(' ', 3);
                if (node.LeftNode != null)
                {
                    GetTree(node.LeftNode, indent);
                }
                Console.WriteLine(indent + node.Space.GetShowString());
                if (node.RightNode != null)
                {
                    GetTree(node.RightNode, indent);
                    // Console.WriteLine($"Правое поддерево, rectangle : ({node.Space.xl}, {node.Space.yl}), ({node.Space.xr}, {node.Space.yr})");
                }
            }
            //Console.WriteLine($"Корень, rectangle : ({node.Space.xl}, {node.Space.yl}), ({node.Space.xr}, {node.Space.yr})");
        }

        /// <summary>
        /// Rectangle создается для каждой Node сразу после создания
        /// </summary>
        /// <param name="node"></param>
        /// <param name="edges"></param>
        public void BuildTree (ref Node node, List<Edge> edges)
        {
            if (edges == null || edges.Count <= 4)  // Точка выхода
            {
                return;
            } 

            
            if (this.Root == null)
            {
                this.Root = node;
            }

            node.Edges = edges; // Возможно переделать и сделать метод, который полностью копирует? // Теперь добавляется полный список граней. Избыток инфы?
            node.FindSegment();

            List<Edge> left = new List<Edge>();
            List<Edge> right = new List<Edge>();
            left.Add(node.Segment);
            right.Add(node.Segment);

            foreach (Edge edge in edges)
            {
                int result = node.ClassifyEdge(edge);

                switch (result)
                {
                    case 0:
                        // Что делаем?
                        break;
                    case 1:
                        left.Add(edge);
                        break;
                    case 2:
                        right.Add(edge);
                        break;
                    case 3:
                        Edge leftEdge = edge.Copy();
                        Edge rightEdge = edge.Copy();
                        edge.SplitEdge(node.Segment, node.Horizontal, ref leftEdge, ref rightEdge);
                        left.Add(leftEdge);
                        right.Add(rightEdge);
                        break;
                }
            }
            // Console.WriteLine($"Segment: ({node.Segment.Left.x}, {node.Segment.Left.y}) ({node.Segment.Right.x}, {node.Segment.Right.y})");
            // Console.WriteLine($"Space: ({node.Space.xl}, {node.Space.yl}) ({node.Space.xr}, {node.Space.yr})");
            if (left != null)
            {
                /*
                foreach(Edge edge in left)
                {
                    Console.WriteLine($"left: ({edge.Left.x}, {edge.Left.y}) ({edge.Right.x}, {edge.Right.y})");
                }
                Console.WriteLine("");
                */

                Node newNode = new Node(left);
                node.LeftNode = newNode;
                BuildTree(ref newNode, left);
            }
            if (right != null)
            {
                /*
                foreach (Edge edge in right)
                {
                    Console.WriteLine($"right: ({edge.Left.x}, {edge.Left.y}) ({edge.Right.x}, {edge.Right.y})");
                }
                Console.WriteLine("");
                */

                Node newNode = new Node(right);
                node.RightNode = newNode;
                BuildTree(ref newNode, right);
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

        public Node() { }

        /// <summary>
        /// Перегрузка оператора инициализации с произвольным числом Edges граней (кратное 4). Минимальное количество граней 4.
        /// </summary>
        /// <returns>
        /// Создается объект Node содержащие Rectangle с первыми 4 гранями массива Edges
        /// </returns>
        public Node(List<Edge> edges)
        {
            this.AddRectangle(edges);
        }

        public void AddRectangle(List<Edge> edges)
        {
            // Создание Rectangle
            List<Edge> tempEdge = SplitListEdges(edges);
            Space = new Rectangle(tempEdge);
        }

        public void FindSegment()
        {
            if (Edges != null && Space != null)
            {
                Edge seg = Edges[4];

                if (seg.Left.x == seg.Right.x)
                {
                    // Vertical
                    this.Segment = seg.CreateVerticalEdge(seg.Left.x, Space.yl, Space.yr);
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
                    if (edge.Left.y < Segment.Left.y && edge.Right.y <= Segment.Left.y)
                    {
                        result = 1;
                    }
                    else if(edge.Left.y >= Segment.Left.y && edge.Right.y > Segment.Left.y)
                    {
                        result = 2;
                    }
                    else if((edge.Left.y > Segment.Left.y && edge.Right.y < Segment.Left.y) || (edge.Left.y < Segment.Left.y && edge.Right.y > Segment.Left.y))
                    {
                        result = 3;
                    }
                }
                else
                {
                    if (edge.Left.x < Segment.Left.x && edge.Right.x <= Segment.Left.x)
                    {
                        result = 1;
                    }
                    else if (edge.Left.x >= Segment.Left.x && edge.Right.x > Segment.Left.x)
                    {
                        result = 2;
                    }
                    else if ((edge.Left.x > Segment.Left.x && edge.Right.x < Segment.Left.x) || (edge.Left.x < Segment.Left.x && edge.Right.x > Segment.Left.x))
                    {
                        result = 3;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Отделяет первые 4 Edge для создания Rectangle из общего списка
        /// </summary>
        /// <param name="edges">Произвольное количество граней Edge. Количество должно быть >= 4</param>
        /// <returns>Возвращает 4 грани для создания Rectangle</returns>
        public List<Edge> SplitListEdges(List<Edge> edges)
        {
            List<Edge> result = new List<Edge>();  // Массив граней образующих Space
            for (int i = 0; i < 4; i++)
            {
                Edge edge = edges[i].Copy();
                result.Add(edge);
            }
            return result;
        }
    }

    class Rectangle
    {
        // xl - левая нижняя
        // xr - верхняя правая
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
            int xmin = edges[0].Left.x;
            int ymin = edges[0].Left.y;
            int ymax = edges[0].Left.y;
            int xmax = edges[0].Left.x;
            Edges = new List<Edge>();
            for (int i = 0; i < edges.Count; i++)
            {
                Edges.Add(edges[i]);
                if(edges[i].Left.x <= xmin) 
                {
                    xmin = edges[i].Left.x;
                    if (edges[i].Left.y < ymin) { ymin = edges[i].Left.y; }
                }
                if (edges[i].Left.x >= xmax) 
                {
                    xmax = edges[i].Left.x;
                    if (edges[i].Left.y > ymax) { ymax = edges[i].Left.y; }
                }
                if (edges[i].Right.x <= xmin) 
                {
                    xmin = edges[i].Right.x;
                    if (edges[i].Right.y < ymin) { ymin = edges[i].Right.y; }
                }
                if (edges[i].Right.x >= xmax) 
                {
                    xmax = edges[i].Right.x;
                    if (edges[i].Right.y > ymax) { ymax = edges[i].Right.y; }
                }
            }

            this.xl = xmin;
            this.xr = xmax;
            this.yl = ymin;
            this.yr = ymax;
            this.SetWidth(xr, xl);
            this.SetHeight(yr, yl);
        }

        public void SetWidth(int x1, int x2)
        {
            this.Width = Math.Abs(x1 - x2);
        }
        public void SetHeight(int y1, int y2)
        {
            this.Height = Math.Abs(y1 - y2);
        }
        public void Show()
        {
            Console.WriteLine($"Rectangle : ({this.xl}, {this.yl}), ({this.xr}, {this.yr})");
        }

        public string GetShowString()
        {
            return "Rectangle: (" + this.xl + ", " + this.yl +"), (" + this.xr + ", " + this.yr + ")";
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

        public Edge(int x1, int y1, int x2, int y2)
        {
            Point left;
            Point right;
            if (x1 < x2)
            {
                left = new Point(x1, y1);
                right = new Point(x2, y2);
            }
            else if (x1 > x2)
            {
                right = new Point(x1, y1);
                left = new Point(x2, y2);
            }
            else
            {
                if (y1 < y2)
                {
                    left = new Point(x1, y1);
                    right = new Point(x2, y2);
                }
                else
                {
                    right = new Point(x1, y1);
                    left = new Point(x2, y2);
                }
            }

            this.Left = left;
            this.Right = right;
        }

        public Edge CreateHorizontalEdge(int y, int xl, int xr)
        {
            Edge edge = new Edge(xl, y, xr, y);
            return edge;
        }

        public Edge CreateVerticalEdge(int x, int yl, int yr)
        {
            Edge edge = new Edge(x, yl, x, yr);
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
                rightEdge.Left.y = splitter.Left.y;
                leftEdge.Right.y = splitter.Left.y;
            }
            else
            {
                // splitter.Left.x == splitter.Right.x
                rightEdge.Left.x = splitter.Left.x;
                leftEdge.Right.x = splitter.Left.x;
            }
        }

        /// <returns>
        /// Возвращает скопированный объект Edge
        /// </returns>
        public Edge Copy()
        {
            Edge edge = new Edge();
            Point left = new Point(this.Left.x, this.Left.y);
            Point right = new Point(this.Right.x, this.Right.y);

            edge.Left = left;
            edge.Right = right;
            return edge;
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
