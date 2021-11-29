using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Automate
{
    public struct Point
    {
        public int x, y;
        public Point(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        public override bool Equals(object obj)
        {
            if (obj is Point p2)
                return x == p2.x && y == p2.y;
            return false;
        }
        public override string ToString()
        {
            return $"{x} {y}";
        }
    }
    public interface ICelluarDrawer
    {
        void RedrawLiveCells(Point[] cells);
        void GameOver();
    }
    public class CelluarAutomat
    {
        ICelluarDrawer drawer;
        bool[,] map;
        int[] live;
        int[] stillLive;

        List<Point> livingCells = new List<Point>();
        public CelluarAutomat(bool[,] map, int[] live, int[] stillLive, ICelluarDrawer drawer)
        {
            this.drawer = drawer;
            this.map = map;
            this.live = live;
            this.stillLive = stillLive;
            for (int x = 0; x < map.GetLength(0); x++)
                for (int y = 0; y < map.GetLength(1); y++)
                    if (map[x, y])
                        livingCells.Add(new Point(x, y));
            drawer.RedrawLiveCells(livingCells.ToArray());
        }
        public void NextIteration()
        {
            if (livingCells.Count == 0)
                drawer.GameOver();

            List<Point> deadCellsAround = new List<Point>();
            List<Point> toKill = new List<Point>();
            foreach (var cell in livingCells)
                CheckAroundLive(cell);
            deadCellsAround = deadCellsAround.Distinct().ToList();

            List<Point> toLive = new List<Point>();
            foreach (var cell in deadCellsAround)
                CheckAroundDeath(cell);

            DoKillAndLive();

            livingCells = livingCells.Distinct().ToList();
            drawer.RedrawLiveCells(livingCells.ToArray());

            void CheckAroundLive(Point cell)
            {
                int neigh = GetNeighbours(cell);
                if (!stillLive.Contains(neigh))
                    toKill.Add(cell);

                int GetNeighbours(Point livingCell)
                {
                    int neighbours = 0;
                    GoTo(-1, -1); GoTo(-1, 0); GoTo(-1, 1);
                    GoTo(0, -1); GoTo(0, 1);
                    GoTo(1, -1); GoTo(1, 0); GoTo(1, 1);
                    return neighbours;

                    void GoTo(int dx, int dy)
                    {
                        if (CheckBoundaries(livingCell.x + dx, livingCell.y + dy))
                            if (map[livingCell.x + dx, livingCell.y + dy])
                                neighbours++;
                            else
                                deadCellsAround.Add(new Point(livingCell.x + dx, livingCell.y + dy));
                    }
                }
            }
            void CheckAroundDeath(Point cell)
            {
                int neigh = GetNeighbours(cell);
                if (live.Contains(neigh))
                    toLive.Add(cell);

                int GetNeighbours(Point deadCell)
                {
                    int neighbours = 0;
                    GoTo(-1, -1); GoTo(-1, 0); GoTo(-1, 1);
                    GoTo(0, -1); GoTo(0, 1);
                    GoTo(1, -1); GoTo(1, 0); GoTo(1, 1);
                    return neighbours;

                    void GoTo(int dx, int dy)
                    {
                        if (CheckBoundaries(deadCell.x + dx, deadCell.y + dy))
                            if (map[deadCell.x + dx, deadCell.y + dy])
                                neighbours++;
                    }
                }
            }
            void DoKillAndLive()
            {
                foreach (var l in toLive)
                {
                    map[l.x, l.y] = true;
                    livingCells.Add(l);
                }

                foreach (var k in toKill)
                {
                    map[k.x, k.y] = false;
                    livingCells.Remove(k);
                }
            }
        }
        private bool CheckBoundaries(int x, int y) => x >= 0 && x < map.GetLength(0) && y >= 0 && y < map.GetLength(1);
    }
}
