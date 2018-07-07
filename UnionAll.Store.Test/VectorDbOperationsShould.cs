using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using UnionAll.Domain;
using UnionAll.Store.Services;
using System.Threading.Tasks;

namespace UnionAll.Store.Test
{
    [TestClass]
    public class VectorDbOperationsShould
    {
        private static DbContextOptionsBuilder _opt;
        private static Node _node1;
        private static Node _object1;
        private static Node _root1;
        private static Node _parent1;
        private static DataRequestParams _reqParams;

        [ClassInitialize]
        public static void InitializeTests(TestContext tc)
        {
            _opt = new DbContextOptionsBuilder<DataContext>().UseInMemoryDatabase("VectorTests");

            _node1 = new Node
            {
                NodeName = "France",
                NodeType = NodeValueTypes.Name,
                NodeTopic = NodeTopics.Country
            };
            _object1 = new Node
            {
                NodeName = "3",
                NodeType = NodeValueTypes.Percentage,
                NodeTopic = NodeTopics.Economic
            };
            _root1 = new Node
            {
                NodeName = "Root",
                NodeType = NodeValueTypes.Name,
                NodeTopic = NodeTopics.Political
            };
            _parent1 = new Node
            {
                NodeName = "Parent",
                NodeType = NodeValueTypes.Name,
                NodeTopic = NodeTopics.Economic
            };

            _reqParams = new DataRequestParams
            {
                PageNumber = 1,
                PageSize = 4
            };
        }

        [ClassCleanup]
        public static void CleanupTests()
        {
            _opt = null;
        }

        [TestMethod]
        public async Task SaveAndSelectOneVector()
        {
            int _vectorCount = 0;
            Vector _saveResult;
            //arrange
            Vector _vector = new Vector
            {
                VectorPhrase = "Empty Vector"
            };
            //act
            using (var ctxSave = new DataContext(_opt.Options))
            {
                VectorRepository _db = new VectorRepository(ctxSave);
                _saveResult = await _db.InsertVectorAsync(_vector);
            }
            using (var ctxRead = new DataContext(_opt.Options))
            {
                VectorRepository _db = new VectorRepository(ctxRead);
                _vectorCount = (_db.SelectVectorAsync(1, _saveResult.VectorId) != null ? 1 : 0);
            }
            //assert
            Assert.AreEqual(1, _vectorCount);
        }

        [TestMethod]
        public async Task GetVectorsByNode()
        {
            Node _node;
            int _vectorCount = 0;
            //arrange
            using (var ctxSave = new DataContext(_opt.Options))
            {
                NodeRepository _db = new NodeRepository(ctxSave);
                await _db.InsertNodeAsync(_node1);
            }
            using (var ctxRead = new DataContext(_opt.Options))
            {
                _node = (
                    from n in ctxRead.Nodes
                    where n.NodeName == "France"
                    select n
                    ).FirstOrDefault();
            }

            Vector _vector = new Vector
            {
                VectorPhrase = "Gross Domestic Product",
                NodeSubject = _node.NodeId
            };
            using (var ctxSave = new DataContext(_opt.Options))
            {
                VectorRepository _db = new VectorRepository(ctxSave);
                await _db.InsertVectorAsync(_vector);
            }

            //act
            using (var ctxReadVector = new DataContext(_opt.Options))
            {
                VectorRepository _db = new VectorRepository(ctxReadVector);
                _vectorCount = (
                    from v in await _db.SelectVectorsByNodeAsync(_node.NodeId, _reqParams)
                    select v
                    ).ToList().Count();
            }
            //assert
            Assert.AreEqual(1, _vectorCount);
        }

    }
}
