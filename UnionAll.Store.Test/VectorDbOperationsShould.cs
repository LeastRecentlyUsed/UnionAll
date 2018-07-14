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
        private static Node _node2;
        private static Node _node3;
        private static Node _node4;
        private static Node _node5;
        private static Node _node6;
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
            _node2 = new Node
            {
                NodeName = "3",
                NodeType = NodeValueTypes.Percentage,
                NodeTopic = NodeTopics.Economic
            };
            _node3 = new Node
            {
                NodeName = "Belgium",
                NodeType = NodeValueTypes.Name,
                NodeTopic = NodeTopics.Political
            };
            _node4 = new Node
            {
                NodeName = "GDP",
                NodeType = NodeValueTypes.Name,
                NodeTopic = NodeTopics.Economic
            };
            _node5 = new Node
            {
                NodeName = "Employment",
                NodeType = NodeValueTypes.Name,
                NodeTopic = NodeTopics.Economic
            };
            _node6 = new Node
            {
                NodeName = "Canada",
                NodeType = NodeValueTypes.Name,
                NodeTopic = NodeTopics.Country
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
            int _nodeId = 0;
            Vector _saveResult;
            //arrange
            Vector _vector = new Vector
            {
                VectorPhrase = "Empty Vector"
            };
            using (var ctxSave = new DataContext(_opt.Options))
            {
                NodeRepository _db = new NodeRepository(ctxSave);
                await _db.InsertNodeAsync(_node1);
            }
            using (var ctxReader = new DataContext(_opt.Options))
            {
                VectorRepository _db = new VectorRepository(ctxReader);
                _nodeId = (
                    from n in ctxReader.Nodes
                    where n.NodeName == "France"
                    select n.NodeId
                    ).FirstOrDefault();
            }
            _vector.NodeSubject = _nodeId;
            //act
            using (var ctxSaveVector = new DataContext(_opt.Options))
            {
                VectorRepository _db = new VectorRepository(ctxSaveVector);
                _saveResult = await _db.InsertVectorAsync(_vector);
            }
            using (var ctxReadVector = new DataContext(_opt.Options))
            {
                VectorRepository _db = new VectorRepository(ctxReadVector);
                _vectorCount = (_db.SelectVectorAsync(_nodeId, _saveResult.VectorId) != null ? 1 : 0);
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
            using (var ctxSaveNode = new DataContext(_opt.Options))
            {
                NodeRepository _db = new NodeRepository(ctxSaveNode);
                await _db.InsertNodeAsync(_node2);
            }
            using (var ctxReadNode = new DataContext(_opt.Options))
            {
                _node = (
                    from n in ctxReadNode.Nodes
                    where n.NodeName == "3"
                    select n
                    ).FirstOrDefault();
            }

            Vector _vector1 = new Vector
            {
                VectorPhrase = "Gross Domestic Product",
                NodeSubject = _node.NodeId
            };
            Vector _vector2 = new Vector
            {
                VectorPhrase = "in 2017",
                NodeSubject = _node.NodeId
            };
            using (var ctxSave = new DataContext(_opt.Options))
            {
                VectorRepository _db = new VectorRepository(ctxSave);
                await _db.InsertVectorAsync(_vector1);
                await _db.InsertVectorAsync(_vector2);
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
            Assert.AreEqual(2, _vectorCount);
        }

        [TestMethod]
        public async Task InsertAndSelectTheSameSetOfVectors()
        {
            Node _node;
            IEnumerable<Vector> _vectorSet = new List<Vector>();
            IEnumerable<Vector> _savedVectors;
            IEnumerable<Vector> _selectedVectors;
            IEnumerable<int> _vectorIdList;

            //arrange
            using (var ctxSaveNode = new DataContext(_opt.Options))
            {
                NodeRepository _db = new NodeRepository(ctxSaveNode);
                await _db.InsertNodeAsync(_node3);
            }
            using (var ctxReadNode = new DataContext(_opt.Options))
            {
                _node = (
                    from n in ctxReadNode.Nodes
                    where n.NodeName == "Belgium"
                    select n
                    ).FirstOrDefault();
            }

            Vector _vector3 = new Vector
            {
                VectorPhrase = "Is a Benelux country",
                NodeSubject = _node.NodeId
            };
            _vectorSet.Append(_vector3);
            _vectorSet.Append(_vector3);
            Vector _vector4 = new Vector
            {
                VectorPhrase = "Has a name starting with a B",
                NodeSubject = _node.NodeId
            };
            _vectorSet.Append(_vector4);
            _vectorSet.Append(_vector4);
            Vector _vector5 = new Vector
            {
                VectorPhrase = "Produces some chocolate",
                NodeSubject = _node.NodeId
            };
            _vectorSet.Append(_vector5);
            _vectorSet.Append(_vector5);

            //act
            using (var ctxSave = new DataContext(_opt.Options))
            {
                VectorRepository _db = new VectorRepository(ctxSave);
                _savedVectors = await _db.InsertVectorSetAsync(_vectorSet);
            }
            _vectorIdList = _savedVectors.Select(x => x.VectorId);

            using (var ctxReadSet = new DataContext(_opt.Options))
            {
                VectorRepository _db = new VectorRepository(ctxReadSet);
                _selectedVectors = await _db.SelectVectorSetAsync(_vectorIdList);
            }

            //assert
            Assert.AreEqual(_savedVectors.Count(), _vectorSet.Count(),
                "The ceated vectors is not the same number as those returned from the save method");
            Assert.AreEqual(_selectedVectors.Count(), _savedVectors.Count(),
                "The selected vector set is not the same count as the inserted vector set");
        }

        [TestMethod]
        public async Task UpdateOneVector()
        {
            Node _node;
            Vector _vectorToUpdate;
            Vector _updatedVector;

            // arrange
            using (var ctxSave = new DataContext(_opt.Options))
            {
                NodeRepository _db = new NodeRepository(ctxSave);
                await _db.InsertNodeAsync(_node5);
            }
            using (var ctxReadNode = new DataContext(_opt.Options))
            {
                _node = (
                    from n in ctxReadNode.Nodes
                    where n.NodeName == "Employment"
                    select n
                    ).FirstOrDefault();
            }

            Vector _newVector = new Vector
            {
                VectorPhrase = "Not Updated",
                VectorStatus = VectorStatusValues.Active,
                NodeSubject = _node.NodeId,
                NodeObject = 100,
                NodeParent = 110,
                NodeRoot = 120
            };
            using (var ctxSave = new DataContext(_opt.Options))
            {
                VectorRepository _db = new VectorRepository(ctxSave);
                _vectorToUpdate = await _db.InsertVectorAsync(_newVector);
            }

            // act
            _vectorToUpdate.HasEdits = true;
            _vectorToUpdate.VectorPhrase = "Updated";
            _vectorToUpdate.NodeObject = 200;
            _vectorToUpdate.NodeParent = 210;

            using (var ctxUpdate = new DataContext(_opt.Options))
            {
                VectorRepository _db = new VectorRepository(ctxUpdate);
                await _db.UpdateVectorAsync(_vectorToUpdate);
            }

            using (var ctxReadVector = new DataContext(_opt.Options))
            {
                _updatedVector = (
                    from v in ctxReadVector.Vectors
                    where v.VectorId == _vectorToUpdate.VectorId
                    select v
                    ).FirstOrDefault();
            }

            // assert
            Assert.AreEqual(_updatedVector.VectorPhrase, _vectorToUpdate.VectorPhrase);
            Assert.AreNotEqual(_updatedVector.NodeObject, 100);
            Assert.AreNotEqual(_updatedVector.NodeParent, 110);
            Assert.AreEqual(_updatedVector.NodeRoot, _vectorToUpdate.NodeRoot);
        }

        [TestMethod]
        public async Task DeleteOneVector()
        {
            Node _node;
            Vector _activeVector;
            Vector _deletedVector;
            Vector _unretrievableVector;

            //arrange
            using (var ctxSave = new DataContext(_opt.Options))
            {
                NodeRepository _db = new NodeRepository(ctxSave);
                await _db.InsertNodeAsync(_node6);
            }
            using (var ctxReadNode = new DataContext(_opt.Options))
            {
                _node = (
                    from n in ctxReadNode.Nodes
                    where n.NodeName == "Canada"
                    select n
                    ).FirstOrDefault();
            }

            Vector _newVector = new Vector
            {
                VectorPhrase = "Not Updated",
                VectorStatus = VectorStatusValues.Active,
                NodeSubject = _node.NodeId
            };
            using (var ctxSave = new DataContext(_opt.Options))
            {
                VectorRepository _db = new VectorRepository(ctxSave);
                _activeVector = await _db.InsertVectorAsync(_newVector);
            }

            // act
            _activeVector.VectorStatus = VectorStatusValues.Deleted;
            _activeVector.HasEdits = true;

            using (var ctxDelete = new DataContext(_opt.Options))
            {
                VectorRepository _db = new VectorRepository(ctxDelete);
                _deletedVector = await _db.DeleteVectorAsync(_activeVector);
            }
            using (var ctxRead = new DataContext(_opt.Options))
            {
                VectorRepository _db = new VectorRepository(ctxRead);
                _unretrievableVector = await _db.SelectVectorAsync(_node.NodeId, _deletedVector.VectorId);
            }
            // assert
            Assert.IsNotNull(_activeVector);
            Assert.AreEqual(VectorStatusValues.Deleted, _deletedVector.VectorStatus);
            Assert.IsNull(_unretrievableVector);
        }

    }
 }
