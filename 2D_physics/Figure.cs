﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace _2D_physics
{
    //線分情報保持用の構造体
    struct Line
    {
        public PointF start;
        public PointF end;
        public int[] suf;//添え字保存
        public Line(PointF start, PointF end, int[] suf)
        {
            this.start = start;
            this.end = end;
            this.suf = suf;
        }
        public Line(PointF start, PointF end)
        {
            this.start = start;
            this.end = end;
            suf = null;
        }

    }
    struct Triangle
    {
        public PointF center;
        public double weight;
        public List<PointF> points;
        public Triangle(PointF p1, PointF p2, PointF p3)
        {
            this.center = new PointF((p1.X + p2.X + p3.X) / 3, (p1.Y + p2.Y + p3.Y) / 3);
            this.weight = Math.Abs(
                p1.Y * (p2.X - p3.X) + p2.Y * (p3.X - p1.X) + p3.Y * (p1.X - p2.X)) / 2;

            this.points = new List<PointF>();
            this.points.Add(p1);
            this.points.Add(p2);
            this.points.Add(p3);
        }
    }

    //図形情報のbean
    //初期化以外の演算はこの中では極力行わない
    class Figure
    {
        public List<PointF> RelatePoints { get; set; }//各点の重心からの相対位置
        public List<PointF> Points
        {
            //各点の絶対位置　描画用？
            //毎回相対位置を参照して計算するので動作が遅いかも 性能面で問題が出るなら要検討
            get
            {
                //Debug中に時たまInvalidOperationExceptionを吐く
                //オブジェクトが破棄された後にアクセスしに行った際の例外処理を書いておく必要？
                List<PointF> retPoints = new List<PointF>();
                RelatePoints.ForEach(point =>
                    retPoints.Add(PointF.Add(point, new SizeF(Center))));
                return retPoints;
            }
            set
            {
                List<PointF> inPoints = value;
                RelatePoints.Clear();
                inPoints.ForEach(point =>
                    RelatePoints.Add(PointF.Add(point, new SizeF(Center))));
            }
        }
        public List<Line> Lines
        {
            //図形を構成する線を返す
            get
            {
                List<Line> lines = new List<Line>();
                for (int i = 0; i < Points.Count; i++)
                {
                    int next = (i + 1) % Points.Count;
                    lines.Add(new Line(Points[i],
                                Points[next],
                                new int[] {i, next}
                                ));
                }
                return lines;
            }

        }
        public List<Triangle> Triangles
        {
            //図形を構成する三角形を返す
            get
            {
                List<Triangle> triangles = new List<Triangle>();

                for (int i = 2; i < Points.Count; i++)
                {
                    triangles.Add(new Triangle(Points[0], Points[i - 1], Points[i]));
                }
                return triangles;
            }

        }


        public PointF Center { get; set; }//（絶対）重心位置

        public PointF Vel { get; set; }//速度
        public double Angv { get; set; }//角速度

        public double Weight { get; set; }//質量
        public double Moment { get; set; }//慣性能率

        public Brush DrawBrush { get; set; }//色　とりあえずはデバッグ用に


        //コンストラクタ
        public Figure(List<PointF> InitialPoints, PointF Center, PointF Vel, double Angv)
        {
            this.GenerateRelatePoints(InitialPoints);
            this.Center = Center;
            this.Vel = Vel;
            this.Angv = Angv;
            this.InitializeWeights();

            this.DrawBrush = Brushes.Coral;
        }

        //RelatePointsの初期化用
        private void GenerateRelatePoints(List<PointF> InitialPoints)
        {
            RelatePoints = new List<PointF>();

            //ローカル座標内の重心位置計算
            PointF centerTemp = new PointF();
            InitialPoints.ForEach(point =>
            {
                centerTemp.X += point.X / InitialPoints.Count;
                centerTemp.Y += point.Y / InitialPoints.Count;
            });

            //重心位置に基づき各点の相対位置を更新
            InitialPoints.ForEach(point =>
                RelatePoints.Add(new PointF(point.X - centerTemp.X, point.Y - centerTemp.Y))
                );
        }
        //質量と慣性モーメントを初期化
        //凹ではない前提で三角形に切り分けて計算する
        private void InitializeWeights()
        {
            Weight = 0;
            Moment = 0;

            Triangle triangle;
            for (int i = 2; i < RelatePoints.Count; i++)
            {
                triangle = new Triangle(RelatePoints[0], RelatePoints[i-1], RelatePoints[i]);
                Weight += triangle.weight;
                Moment += triangle.weight * MyMath.Distance(new PointF(0,0), triangle.center);
            }

        }

    }
}
