using HotelOS.Domain.Masters.Entities;
using HotelOS.Domain.Workflow.Entities;
using HotelOS.Infrastructure.Persistence;
using HotelOS.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HotelOS.UnitTests;

public class CoreEnginesTests
{
    [Fact]
    public async Task DropdownEngine_Should_Enforce_Max_5_Records_Per_Batch_And_Support_Show_More()
    {
        var options = new DbContextOptionsBuilder<HotelOSDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new HotelOSDbContext(options);
        var masterService = new MasterService(context);

        // Seed Master Definition: DEPARTMENT
        var cat = new MasterCategory { Code = "ORG", Name = "Org" };
        context.MasterCategories.Add(cat);
        var def = new MasterDefinition { CategoryId = cat.Id, Code = "DEPARTMENT", Name = "Department" };
        context.MasterDefinitions.Add(def);

        // Add 12 departments
        for (int i = 1; i <= 12; i++)
        {
            context.MasterValues.Add(new MasterValue
            {
                MasterDefinitionId = def.Id,
                Code = $"DEPT_{i:D2}",
                Name = $"Department {i:D2}",
                DisplayOrder = i,
                IsActive = true
            });
        }
        await context.SaveChangesAsync();

        // 1. Mandatory rule: initial load requests without size or with size > 5 must be capped to 5!
        var batch1 = await masterService.GetDropdownOptionsAsync("DEPARTMENT", page: 1, pageSize: 50, search: null);

        Assert.Equal(5, batch1.Items.Count); // Strictly capped to 5!
        Assert.Equal(12, batch1.TotalCount);
        Assert.Equal(1, batch1.Page);
        Assert.True(batch1.HasMore); // Has more records for "Show More"

        // 2. Fetch next 5 records (Show More / Page 2)
        var batch2 = await masterService.GetDropdownOptionsAsync("DEPARTMENT", page: 2, pageSize: 5, search: null);

        Assert.Equal(5, batch2.Items.Count);
        Assert.Equal(2, batch2.Page);
        Assert.True(batch2.HasMore);
        Assert.Equal("Department 06", batch2.Items[0].Name);

        // 3. Fetch final 2 records (Page 3)
        var batch3 = await masterService.GetDropdownOptionsAsync("DEPARTMENT", page: 3, pageSize: 5, search: null);

        Assert.Equal(2, batch3.Items.Count);
        Assert.Equal(3, batch3.Page);
        Assert.False(batch3.HasMore); // No more records

        // 4. Server-side search filtering
        var searchResult = await masterService.GetDropdownOptionsAsync("DEPARTMENT", page: 1, pageSize: 5, search: "08");
        Assert.Single(searchResult.Items);
        Assert.Equal("Department 08", searchResult.Items[0].Name);
    }

    [Fact]
    public async Task WorkflowEngine_Should_Advance_Steps_And_Enforce_Transitions()
    {
        var options = new DbContextOptionsBuilder<HotelOSDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new HotelOSDbContext(options);
        var workflowService = new WorkflowService(context);

        // Setup test workflow
        var wf = new WorkflowDefinition
        {
            Code = "TEST_REQ",
            Title = "Test Requisition",
            Module = "Purchase",
            Version = 1,
            IsPublished = true
        };
        context.WorkflowDefinitions.Add(wf);
        await context.SaveChangesAsync();

        var step1 = new WorkflowStep { WorkflowDefinitionId = wf.Id, StepOrder = 1, StepName = "Draft", StepType = "Start" };
        var step2 = new WorkflowStep { WorkflowDefinitionId = wf.Id, StepOrder = 2, StepName = "Manager Approval", StepType = "Approval" };
        var step3 = new WorkflowStep { WorkflowDefinitionId = wf.Id, StepOrder = 3, StepName = "Completed", StepType = "End" };
        context.WorkflowSteps.AddRange(step1, step2, step3);
        await context.SaveChangesAsync();

        context.WorkflowTransitions.AddRange(
            new WorkflowTransition { WorkflowDefinitionId = wf.Id, FromStepId = step1.Id, ToStepId = step2.Id, ActionName = "Submit" },
            new WorkflowTransition { WorkflowDefinitionId = wf.Id, FromStepId = step2.Id, ToStepId = step3.Id, ActionName = "Approve" },
            new WorkflowTransition { WorkflowDefinitionId = wf.Id, FromStepId = step2.Id, ToStepId = step1.Id, ActionName = "Reject" }
        );
        await context.SaveChangesAsync();

        // 1. Start workflow
        var instance = await workflowService.StartWorkflowAsync("TEST_REQ", "Requisition", 999, userId: 1);
        Assert.NotNull(instance);
        Assert.Equal("Draft", instance.CurrentStepName);
        Assert.Equal("InProgress", instance.Status);
        Assert.Contains("Submit", instance.AvailableActions);

        // 2. Submit to Step 2
        var transitioned1 = await workflowService.TransitionStepAsync(instance.Id, "Submit", "Submitting for approval", userId: 1);
        Assert.Equal("Manager Approval", transitioned1.CurrentStepName);
        Assert.Equal("InProgress", transitioned1.Status);
        Assert.Contains("Approve", transitioned1.AvailableActions);
        Assert.Contains("Reject", transitioned1.AvailableActions);

        // 3. Invalid action should throw
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            workflowService.TransitionStepAsync(instance.Id, "RandomAction", null, userId: 1));

        // 4. Approve to Final Step
        var approved = await workflowService.TransitionStepAsync(instance.Id, "Approve", "Approved by Manager", userId: 2);
        Assert.Equal("Completed", approved.CurrentStepName);
        Assert.Equal("Approved", approved.Status);
        Assert.Empty(approved.AvailableActions); // Finished
        Assert.Equal(3, approved.History.Count);
    }
}
