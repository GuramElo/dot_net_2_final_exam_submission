using FluentAssertions;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Core.Types;
using Reddit.Models;
using Reddit.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Reddit.UnitTests.Tests
{
    public class PagedListTests
    {
        private ApplicationDbContext CreateContext(int numberOfItems)
        {
            var dbName = Guid.NewGuid().ToString();     // unique dbname
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            var context = new ApplicationDbContext(options);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            for (int i = 0; i < numberOfItems; i++)
            {
                context.Add(new Post { Id = i + 1, Title = (i + 1) + "st Post", Content = (i + 1) + "st Post content", Upvotes = 100, Downvotes = 2 });
            }

            context.SaveChanges();
            return context;
        }
        [Theory]
        [InlineData(2, 5, 2)]
        [InlineData(3, 5, 1)]
        [InlineData(4, 10, 2)]
        [InlineData(1, 5, 3)]
        [InlineData(5, 15, 2)]
        public async Task CreateAsync_ReturnsCorrectItems(int pageSize, int numberOfItems, int currentPage)
        {
            using var context = CreateContext(numberOfItems);
           

            var posts = context.Posts.AsQueryable();
            var pagedList = await PagedList<Post>.CreateAsync(posts, currentPage, pageSize);


            var pagedItemsCount = await posts.Skip((currentPage - 1) * pageSize).Take(pageSize).CountAsync();

            pagedList.Should().BeOfType<PagedList<Post>>();

            pagedList.Items.Should().BeOfType<List<Post>>();

            pagedList.Items.Should().HaveCount(pagedItemsCount);


        }
        [Theory]
        [InlineData(2, 5, 2)]
        [InlineData(3, 5, 1)]
        [InlineData(4, 10, 2)]
        [InlineData(1, 5, 3)]
        [InlineData(5, 15, 2)]
        public async Task CreateAsync_ReturnsCorrectNextPageConfig_And_CorrectPreviousPageConfig(int pageSize, int numberOfItems, int currentPage)
        {
            using var context = CreateContext(numberOfItems);
           

            var posts = context.Posts.AsQueryable();

            var pagedList = await PagedList<Post>.CreateAsync(posts, currentPage, pageSize);

            var shouldHaveNextPage = (currentPage * pageSize) < numberOfItems;
            var shouldHavePreviousPage = currentPage > 1;

            pagedList.HasPreviousPage.Should().Be(shouldHavePreviousPage);

            pagedList.HasNextPage.Should().Be(shouldHaveNextPage);


        }

        [Theory]
        [InlineData(2, 0, 2)]
        [InlineData(3, 0, 2)]
        [InlineData(4, 0, 2)]
        [InlineData(1, 0, 3)]
        [InlineData(5, 0, 2)]
        public async Task CreateAsync_Behaviour_WhenListIsEmpty(int pageSize, int numberOfItems, int currentPage)
        {
            using var context = CreateContext(numberOfItems);
       

            var posts = context.Posts.AsQueryable();
            //ეს გააზრებულად ფეილდება, არ აბრუნებს სწროად დათას და მაგიტომ
            var pagedList = await PagedList<Post>.CreateAsync(posts, currentPage, pageSize);

            var shouldHaveNextPage = (currentPage * pageSize) < numberOfItems;
            var shouldHavePreviousPage = currentPage > 1;

            pagedList.HasPreviousPage.Should().Be(shouldHavePreviousPage);

            pagedList.HasNextPage.Should().Be(shouldHaveNextPage);

            var pagedItemsCount = await posts.Skip((currentPage - 1) * pageSize).Take(pageSize).CountAsync();

            pagedList.Should().BeOfType<PagedList<Post>>();

            pagedList.Items.Should().BeOfType<List<Post>>();

            pagedList.Items.Should().HaveCount(pagedItemsCount);

        }

        [Theory]
        [InlineData(2, 5, 6)]
        [InlineData(3, 5, 6)]
        [InlineData(4, 10, 11)]
        [InlineData(1, 5, 6)]
        [InlineData(5, 15, 16)]
        public async Task CreateAsync_Behaviour_WhenPageSizeIsLargerThanTotalCount(int pageSize, int numberOfItems, int currentPage)
        {
            using var context = CreateContext(numberOfItems);
         

            var posts = context.Posts.AsQueryable();

            //ეს არ ფეილდება, როცა pageSize totalCount-ზე მეტია, ეგ პოზიტიურად მთავრდება


            var pagedList = await PagedList<Post>.CreateAsync(posts, currentPage, pageSize);

            var shouldHaveNextPage = (currentPage * pageSize) < numberOfItems;
            var shouldHavePreviousPage = currentPage > 1;

            pagedList.HasPreviousPage.Should().Be(shouldHavePreviousPage);

            pagedList.HasNextPage.Should().Be(shouldHaveNextPage);

            var pagedItemsCount = await posts.Skip((currentPage - 1) * pageSize).Take(pageSize).CountAsync();

            pagedList.Should().BeOfType<PagedList<Post>>();

            pagedList.Items.Should().BeOfType<List<Post>>();

            pagedList.Items.Should().HaveCount(pagedItemsCount);
        }

        [Theory]
        [InlineData(2, 5, 2)]
        [InlineData(3, 5, 1)]
        [InlineData(4, 10, 2)]
        [InlineData(1, 5, 3)]
        [InlineData(5, 15, 2)]
        public async Task CreateAsync_Behaviour_WhenTotalCountIsMoreThanPageSize(int pageSize, int numberOfItems, int currentPage)
        {
            using var context = CreateContext(numberOfItems);
        

            var posts = context.Posts.AsQueryable();

            //ეს არ ფეილდება, როცა totalCount pageSize-ზე მეტია, ეგ პოზიტიურად მთავრდება


            var pagedList = await PagedList<Post>.CreateAsync(posts, currentPage, pageSize);

            var shouldHaveNextPage = (currentPage * pageSize) < numberOfItems;
            var shouldHavePreviousPage = currentPage > 1;

            pagedList.HasPreviousPage.Should().Be(shouldHavePreviousPage);

            pagedList.HasNextPage.Should().Be(shouldHaveNextPage);

            var pagedItemsCount = await posts.Skip((currentPage - 1) * pageSize).Take(pageSize).CountAsync();

            pagedList.Should().BeOfType<PagedList<Post>>();

            pagedList.Items.Should().BeOfType<List<Post>>();

            pagedList.Items.Should().HaveCount(pagedItemsCount);
        }


        [Theory]
        [InlineData(-1, 5, 0)]
        [InlineData(3, 5, 0)]
        [InlineData(4, 10, 0)]
        [InlineData(1, 5, -1)]
        [InlineData(-1, 15, 0)]
        public async Task CreateAsync_Behaviour_WhenPageOrPageSizeIsSetToNonPositiveOrZero(int pageSize, int numberOfItems, int currentPage)
        {
            using var context = CreateContext(numberOfItems);
          

            var posts = context.Posts.AsQueryable();

            //ეს ფეილდება, რადგან ასეთ დროს უნდა ისროლოს range-ის exception და არ ისვრის


            var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => PagedList<Post>.CreateAsync(posts, currentPage, pageSize));

            Assert.True("page" == exception.ParamName || "pageSize" == exception.ParamName);
        }


        [Theory]
        [InlineData(2, 5, 10)]
        [InlineData(3, 5, 11)]
        [InlineData(4, 10, 20)]
        [InlineData(1, 5, 80)]
        [InlineData(5, 15, 100)]
        public async Task CreateAsync_Behaviour_WhenPageIsSetToValueThatIsOutOfRange(int pageSize, int numberOfItems, int currentPage) // like when it's greater then total number of pages
        {
            using var context = CreateContext(numberOfItems);
           

            var posts = context.Posts.AsQueryable();

            //ესეც ფეილდება, რადგან ასეთ დროს უნდა ისროლოს range-ის exception და არ ისვრის


            var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => PagedList<Post>.CreateAsync(posts, currentPage, pageSize));

            Assert.True("page" == exception.ParamName);
        }
    }
}
