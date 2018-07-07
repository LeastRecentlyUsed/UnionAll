using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using UnionAll.Domain;
using UnionAll.Store.Services;
using System.Threading.Tasks;
using System;

namespace UnionAll.Store.Test
{
    [TestClass]
    public class NodeDbOperationsShould
    {
        private static DbContextOptionsBuilder _opt;
        private static Node _node1;
        private static Node _node2;
        private static Node _node3;
        private static Node _node7;
        private static Node _node8;
        private static DataRequestParams _reqParams;

        [ClassInitialize]
        public static void InitializeTests(TestContext tc)
        {
            _opt = new DbContextOptionsBuilder<DataContext>().UseInMemoryDatabase("NodeTests");

            _node1 = new Node
            {
                NodeName = "France",
                NodeType = NodeValueTypes.Name,
                NodeTopic = NodeTopics.Country
            };
            _node2 = new Node
            {
                NodeName = "Gross Domestic Product",
                NodeType = NodeValueTypes.Measure,
                NodeTopic = NodeTopics.Economic
            };
            _node3 = new Node
            {
                NodeName = "Population",
                NodeType = NodeValueTypes.Measure,
                NodeTopic = NodeTopics.Political
            };
            _node7 = new Node
            {
                NodeName = "Air France",
                NodeType = NodeValueTypes.Name,
                NodeTopic = NodeTopics.Commercial,
                NodeStatus = NodesStatusValues.Active,
                HasEdits = false
            };
            _node8 = new Node
            {
                NodeName = "Perpendicular",
                NodeType = NodeValueTypes.Name,
                NodeTopic = NodeTopics.Research,
                HasEdits = false
            };

            _reqParams = new DataRequestParams
            {
                PageNumber = 1,
                PageSize = 2
            };
        }

        [ClassCleanup]
        public static void CleanupTests()
        {
            _opt = null;
        }

        [TestMethod]
        public async Task SelectNodeListAsValidKeyValuePair()
        {
            // arrange
            using (var ctxSave = new DataContext(_opt.Options))
            {
                NodeRepository _db = new NodeRepository(ctxSave);
                await _db.InsertNodeAsync(_node3);
            }
            // act
            using (var ctx = new DataContext(_opt.Options))
            {
                NodeRepository _db = new NodeRepository(ctx);
                var nodeSubjectList = await _db.SelectNodeListAsync(_reqParams);
                // assert
                Assert.IsInstanceOfType(nodeSubjectList, typeof(IEnumerable<KeyValuePair<int, string>>));
            }
        }

        [TestMethod]
        public async Task InsertNodeSetAndThenSelectAllNodes()
        {
            //arrange
            DataRequestParams _params = _reqParams;
            _params.PageSize = 100;
            IEnumerable<Node> _nodeSetSelected;
            IEnumerable<Node> _nodeSetInserted;
            int _insertCount = 0;
            int _selectCount = 0;
            IEnumerable<Node> _nodeSetToInsert = new List<Node>
            {
                _node1, _node2, _node3, _node7, _node8
            };
            //act
            using (var ctxInsertSet = new DataContext(_opt.Options))
            {
                NodeRepository _db = new NodeRepository(ctxInsertSet);
                _nodeSetInserted = await _db.InsertNodeSetAsync(_nodeSetToInsert);
            }
            _insertCount = _nodeSetToInsert.Count();
            using (var ctxReadSet = new DataContext(_opt.Options))
            {
                NodeRepository _db = new NodeRepository(ctxReadSet);
                _nodeSetSelected = await _db.SelectAllNodesAsync(_params);
            }
            _selectCount = _nodeSetSelected.Count();
            //assert
            Assert.AreEqual(_selectCount, _insertCount,
                $"Inserted Count {_insertCount} is not selected count from DB {_selectCount}");
        }

        [TestMethod]
        public async Task InsertNodeSetAndThenSelectAllPaginatedNodes()
        {
            //arrange
            DataRequestParams _params = _reqParams;
            int _selectCount = 0;
            int _pageCount = 0;
            IEnumerable<Node> _nodeSetToInsert = new List<Node>
            {
                _node1, _node2, _node3, _node7, _node8
            };
            int _insertCount = _nodeSetToInsert.Count();
            int _expectedPageCount = (_insertCount + _params.PageSize -1) / _params.PageSize;
            //act
            using (var ctxInsertSet = new DataContext(_opt.Options))
            {
                NodeRepository _db = new NodeRepository(ctxInsertSet);
                var _ins = await _db.InsertNodeSetAsync(_nodeSetToInsert);
            }
            // act pagination
            using (var ctxReadSet = new DataContext(_opt.Options))
            {
                NodeRepository _db = new NodeRepository(ctxReadSet);
                while (_selectCount < _insertCount)
                {
                    var _sel = await _db.SelectAllNodesAsync(_params);
                    _pageCount++;
                    _selectCount += _sel.Count();
                }
            }
            //assert
            Assert.AreEqual(_expectedPageCount, _pageCount,
                $"Expected {_expectedPageCount} pages but executed ]{_pageCount} pages");
        }

        [TestMethod]
        public async Task InsertOneNode()
        {
            //arrange
            Node _n1 = new Node
            {
                NodeName = "Germany",
                NodeType = NodeValueTypes.Name,
                NodeTopic = NodeTopics.Country,
                HasEdits = false
            };
            //act
            using (var ctxSave = new DataContext(_opt.Options))
            {
                NodeRepository _db = new NodeRepository(ctxSave);
                await _db.InsertNodeAsync(_n1);
            }
            int _nodeCount = 0;
            using (var ctxRead = new DataContext(_opt.Options))
            {
                _nodeCount = (
                    from n in ctxRead.Nodes
                    where n.NodeName == "Germany"
                    select n
                    ).ToList().Count();
            }
            //assert
            Assert.AreEqual(1, _nodeCount);
        }

        [TestMethod]
        public async Task SelectOneNodeByNodeId()
        {
            Node _searchedNode;
            Node _getNode;
            //arrange
            using (var ctxSave = new DataContext(_opt.Options))
            {
                NodeRepository _db = new NodeRepository(ctxSave);
                await _db.InsertNodeAsync(_node1);
                await _db.InsertNodeAsync(_node2);
            }
            using (var ctxRead = new DataContext(_opt.Options))
            {
                _searchedNode = (
                    from n in ctxRead.Nodes
                    where n.NodeName == "France"
                    select n
                    ).FirstOrDefault();
            }
            //act
            using (var ctxGet = new DataContext(_opt.Options))
            {
                NodeRepository _db = new NodeRepository(ctxGet);
                _getNode = await _db.SelectNodeAsync(_searchedNode.NodeId);
            }
            //assert
            Assert.AreEqual(_searchedNode.NodeId, _getNode.NodeId,
                $"No Matching Nodes: [searchedNode {_searchedNode.NodeId}] and [getNode {_getNode.NodeId}]");
        }

        [TestMethod]
        public async Task DeleteOneNode()
        {
            //arrange
            Node _activeNode;
            Node _deletedNode;
            Node _unretrievableNode;

            using (var ctxSave = new DataContext(_opt.Options))
            {
                // insert the node to set the NodeId.
                NodeRepository _db = new NodeRepository(ctxSave);
                await _db.InsertNodeAsync(_node7);
            }
            using (var ctxRead = new DataContext(_opt.Options))
            {
                _activeNode = (
                    from n in ctxRead.Nodes
                    where n.NodeName == "Air France"
                    select n
                    ).FirstOrDefault();
            }
            // act
            _activeNode.NodeStatus = NodesStatusValues.Deleted;
            _activeNode.HasEdits = true;
            using (var ctxDelete = new DataContext(_opt.Options))
            {
                NodeRepository _db = new NodeRepository(ctxDelete);
                _deletedNode = await _db.DeleteNodeAsync(_activeNode);
            }
            using (var ctxRead = new DataContext(_opt.Options))
            {
                NodeRepository _db = new NodeRepository(ctxRead);
                _unretrievableNode = await _db.SelectNodeAsync(_activeNode.NodeId);
            }
            // assert
            Assert.IsNotNull(_activeNode);
            Assert.AreEqual(NodesStatusValues.Deleted, _deletedNode.NodeStatus);
            Assert.IsNull(_unretrievableNode);
        }

        [TestMethod]
        public async Task UpdateOneNode()
        {
            // arrange
            Node _modifiedNode;
            Node _retrievedModifiedNode;
            using (var ctxSave = new DataContext(_opt.Options))
            {
                NodeRepository _db = new NodeRepository(ctxSave);
                await _db.InsertNodeAsync(_node8);
            }
            // act
            using (var ctxRead = new DataContext(_opt.Options))
            {
                _modifiedNode = (
                    from n in ctxRead.Nodes
                    where n.NodeName == "Perpendicular"
                    select n
                    ).FirstOrDefault();
            }
            _modifiedNode.NodeName = "Triangular";
            _modifiedNode.NodeTopic = NodeTopics.Individual;
            _modifiedNode.HasEdits = true;
            using (var ctxDelete = new DataContext(_opt.Options))
            {
                NodeRepository _db = new NodeRepository(ctxDelete);
                _retrievedModifiedNode = await _db.DeleteNodeAsync(_modifiedNode);
            }
            // assert
            Assert.AreEqual(_modifiedNode.NodeId, _retrievedModifiedNode.NodeId);
            Assert.AreEqual(_modifiedNode.NodeType, _retrievedModifiedNode.NodeType);
            Assert.AreNotEqual(_node8.NodeName, _retrievedModifiedNode.NodeName);
            Assert.AreNotEqual(_node8.NodeTopic, _retrievedModifiedNode.NodeTopic);
        }

        [TestMethod]
        public async Task InsertAndSelectTheSameNodeSet()
        {
            // arrange
            IEnumerable<int> _nodeIdSet;
            IEnumerable<Node> _nodeSetSelected;
            IEnumerable<Node> _nodeSetInserted;
            int _insertCount = 0;
            int _selectCount = 0;
            IEnumerable<Node> _nodeSetToInsert = new List<Node>
            {
                _node1, _node2, _node3, _node7
            };
            _insertCount = _nodeSetToInsert.Count();
            // act
            using (var ctxSave = new DataContext(_opt.Options))
            {
                NodeRepository _db = new NodeRepository(ctxSave);
                var _ins = await _db.InsertNodeSetAsync(_nodeSetToInsert);
            }

            var _names = _nodeSetToInsert.Select(x => x.NodeName);

            using (var ctxRead = new DataContext(_opt.Options))
            {
                _nodeSetInserted = (
                    from n in ctxRead.Nodes
                    where _names.Contains(n.NodeName)
                    select n
                    ).ToList();
            }

            _selectCount = _nodeSetInserted.Count();
            _nodeIdSet = _nodeSetInserted.Select(x => x.NodeId);

            if (_nodeIdSet.Count() == 0)
                Assert.Fail("No IDs returned from the Node Search on Node Name");

            using (var ctxCheck = new DataContext(_opt.Options))
            {
                NodeRepository _dbRead = new NodeRepository(ctxCheck);
                _nodeSetSelected = await _dbRead.SelectNodeSetAsync(_nodeIdSet);
            }
            // assert
            //Assert.AreEqual(_nodeSetSelected.Count(), _selectCount);
            Assert.AreEqual(_insertCount, _selectCount);
        }

    }
}
