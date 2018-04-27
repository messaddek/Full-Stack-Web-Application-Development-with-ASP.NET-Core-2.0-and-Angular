using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Macaria.API.Features.Notes;
using Macaria.Core.Entities;
using Macaria.Infrastructure.Data;
using Macaria.Infrastructure.Extensions;
using System.Collections.Generic;
using Macaria.API.Features.Tags;

namespace IntegrationTests.Features
{
    public class NoteScenarios: NoteScenarioBase
    {
        [Fact]
        public async Task ShouldSaveNote()
        {
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            using (var server = CreateServer())
            {
                var hubConnection = GetHubConnection(server.CreateHandler());

                hubConnection.On<dynamic>("message", (result) =>
                {
                    Assert.Equal("[Note] Saved", $"{result.type}");
                    Assert.Equal(1, Convert.ToInt16(result.payload.note.noteId));
                    tcs.SetResult(true);
                });

                await hubConnection.StartAsync();

                var response = await server.CreateClient()
                    .PostAsAsync<SaveNoteCommand.Request, SaveNoteCommand.Response>(Post.Notes, new SaveNoteCommand.Request() {
                        Note = new NoteApiModel()
                        {
                            Title = "First Note",
                            Body = "<p>Something Important</p>",
                            Tags = new List<TagApiModel>() { new TagApiModel() { TagId = 1, Name = "Angular" } }
                        }
                    });

                Assert.True(response.NoteId == 1);

                await tcs.Task;
            }
        }

        [Fact]
        public async Task ShouldGetAllNotes()
        {
            void setUpData(MacariaContext context)
            {
                context.Notes.Add(new Note()
                {
                    Title = "Title1",
                    Body = "Body",
                    
                });

                context.Notes.Add(new Note()
                {
                    Title = "Title2",
                    Body = "Body",
                    
                });

                context.Notes.Add(new Note()
                {
                    Title = "Title3",
                    Body = "Body",
                    
                });

                context.SaveChanges();
            }

            using (var server = CreateServer(setUpData))
            {
                var response = await server.CreateClient()
                    .GetAsync<GetNotesQuery.Response>(Get.Notes);

                Assert.True(response.Notes.Count() == 3);
            }
        }

        [Fact]
        public async Task ShouldGetNoteById()
        {
            void setUpData(MacariaContext context)
            {
                context.Notes.Add(new Note()
                {
                    Title = "Title",
                    Body = "Body",

                });

                context.SaveChanges();
            }

            using (var server = CreateServer(setUpData))
            {
                var response = await server.CreateClient()
                    .GetAsync<GetNoteByIdQuery.Response>(Get.NoteById(1));

                Assert.True(response.Note.NoteId != default(int));
            }
        }

        [Fact]
        public async Task ShouldGetNoteBySlug()
        {
            void setUpData(MacariaContext context)
            {
                context.Notes.Add(new Note()
                {
                    Title = "Title",
                    Body = "Body",
                    Slug = "title"
                });

                context.SaveChanges();
            }

            using (var server = CreateServer(setUpData))
            {
                var response = await server.CreateClient()
                    .GetAsync<GetNoteBySlugQuery.Response>(Get.NoteBySlug("title"));

                Assert.True(response.Note.NoteId != default(int));
            }
        }

        [Fact]
        public async Task ShouldGetNotesByTagSlug()
        {
            void setUpData(MacariaContext context)
            {
                context.Notes.Add(new Note()
                {
                    Title = "Angular Routing",
                    Body = "Body",
                    NoteTags = new List<NoteTag>() {
                        new NoteTag() { TagId = 1 }
                    }
                });

                context.Notes.Add(new Note()
                {
                    Title = "Angular Rendering",
                    Body = "Body",
                    NoteTags = new List<NoteTag>() {
                        new NoteTag() { TagId = 1 }
                    }
                });

                context.SaveChanges();
            }

            using (var server = CreateServer(setUpData))
            {
                var response = await server.CreateClient()
                    .GetAsync<GetNotesByTagSlugQuery.Response>(Get.NoteByTagSlug("angular"));

                Assert.True(response.Notes.Count() == 2);
            }
        }


        [Fact]
        public async Task ShouldUpdateNote()
        {
            void setUpData(MacariaContext context)
            {
                context.Notes.Add(new Note()
                {
                    Title = "Title",
                    Body = "Body",
                });

                context.SaveChanges();
            }

            using (var server = CreateServer(setUpData))
            {
                
                var saveResponse = await server.CreateClient()
                    .PostAsAsync<SaveNoteCommand.Request, SaveNoteCommand.Response>(Post.Notes, new SaveNoteCommand.Request()
                    {
                        Note = new NoteApiModel()
                        {
                            NoteId = 1,
                            Title = "Title",
                            Body = "Body"
                        }
                    });

                Assert.True(saveResponse.NoteId == 1);
            }
        }
        
        [Fact]
        public async Task ShouldDeleteNote()
        {
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            void setUpData(MacariaContext context)
            {
                context.Notes.Add(new Note()
                {
                    Title = "Title",
                    Body = "Body"                    
                });

                context.SaveChanges();
            }

            using (var server = CreateServer(setUpData))
            {
                var hubConnection = GetHubConnection(server.CreateHandler());

                hubConnection.On<dynamic>("message", (result) =>
                {
                    Assert.Equal("[Note] Removed", $"{result.type}");
                    Assert.Equal(1, Convert.ToInt16(result.payload.noteId));
                    tcs.SetResult(true);
                });

                await hubConnection.StartAsync();

                var response = await server.CreateClient()
                    .DeleteAsync(Delete.Note(1));

                response.EnsureSuccessStatusCode();

                await tcs.Task;
            }
        }
    }
}
