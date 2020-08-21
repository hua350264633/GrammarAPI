using Microsoft.VisualStudio.TestTools.UnitTesting;
using GrammarAPI.Controllers;
using System;
using System.Collections.Generic;
using System.Text;
using ML.Dapper.Base;
using ML.Dapper;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections;
using System.Diagnostics;

namespace GrammarAPI.Controllers.Tests
{
    [TestClass()]
    public class UserControllerTests
    {
        [TestMethod()]
        public void GetTest()
        {
            Queue q = new Queue();
            q.Enqueue("a");
            q.Enqueue("b");

            foreach (var item in q)
            {
                Debug.WriteLine(item);
            }
        }

        [TestMethod()]
        public void GetTest1()
        {
            Stack q = new Stack();
            q.Push("c");
            q.Push("d");

            foreach (var item in q)
            {
                Debug.WriteLine(item);
            }
        }

        [TestMethod()]
        public void GetTest2()
        {
            List<int> q = new List<int>();
            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < 2000000; i++)
            {
                q.Add(i);
            }
            sw.Stop();
            Debug.WriteLine($"插入耗时：{sw.ElapsedMilliseconds}毫秒");
            sw.Reset();
            sw.Start();
            for (int i = 200000 - 1; i >= 0; i--)
            {
                q.Remove(i);
            }
            sw.Stop();
            Debug.WriteLine($"移除耗时：{sw.ElapsedMilliseconds}毫秒");
        }
    }
}