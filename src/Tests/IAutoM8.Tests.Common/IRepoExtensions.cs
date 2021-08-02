using IAutoM8.Repository;
using Moq;
using System.Collections.Generic;
using System.Linq;

namespace IAutoM8.Tests.Common
{
    public static class IRepoExtensions
    {
        /// <summary>
        /// Creates a mocked Repository from specified source. See <see cref="IRepo"/>
        /// Additionaly mocks transaction, if provided, to use the same data source. See <see cref="ITransactionScope"/>
        /// </summary>
        public static void SetupRepoMock<T>(this Mock<IRepo> repo,
                                            IEnumerable<T> source,
                                            Mock<ITransactionScope> transactionMock = null)
           where T : class
        {
            var queryableSource = source.AsQueryable();
            var mockSet = new Mock<IQueryable<T>>();

            mockSet.As<IAsyncEnumerable<T>>()
                .Setup(m => m.GetEnumerator())
                .Returns(new TestAsyncEnumerator<T>(queryableSource.GetEnumerator()));

            mockSet
                .Setup(m => m.Provider)
                .Returns(new TestAsyncQueryProvider<T>(queryableSource.Provider));

            mockSet.Setup(m => m.Expression).Returns(queryableSource.Expression);
            mockSet.Setup(m => m.ElementType).Returns(queryableSource.ElementType);
            mockSet.Setup(m => m.GetEnumerator()).Returns(() => queryableSource.GetEnumerator());

            repo.Setup(s => s.Read<T>()).Returns(mockSet.Object);
            repo.Setup(s => s.Track<T>()).Returns(mockSet.Object);

            if (transactionMock != null)
            {
                repo.Setup(s => s.Transaction()).Returns(transactionMock.Object);
                transactionMock.Setup(s => s.Read<T>()).Returns(mockSet.Object);
                transactionMock.Setup(s => s.Track<T>()).Returns(mockSet.Object);
            }
        }

        /// <summary>
        /// Setups repo mock with one item as a data source
        /// </summary>
        public static void SetupSingleValueRepoMock<T>(this Mock<IRepo> repo,
            T source,
            Mock<ITransactionScope> transactionMock = null)
            where T : class
        {
            repo.SetupRepoMock(new List <T> { source }, transactionMock);
        }

        /// <summary>
        /// Setups remo mock with empty data source
        /// </summary>
        public static void SetupRepoMock<T>(this Mock<IRepo> repo,
            Mock<ITransactionScope> transactionMock = null)
            where T : class
        {
            repo.SetupRepoMock(new List<T>(), transactionMock);
        }
    }
}
