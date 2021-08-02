
namespace IAutoM8.Global.Options
{
    public class RolePermissions
    {
        #region Formula

        public bool FormulasPage { get; set; }
        public bool CreateFormula { get; set; }
        public bool EditFormulaInfo { get; set; }
        public bool DeletFormula { get; set; }
        public bool ViewFormula { get; set; }
        public bool AccessToFormulaUiEditor { get; set; }
        public bool CreateProjectFromFormula { get; set; }
        public bool LockFormula { get; set; } //Commented

        public bool FormulaModalWindow { get; set; }
        public bool SetFormulaOwner { get; set; }
        public bool SetFormulaMembers { get; set; }
        public bool AddFormulaFiles { get; set; }
        public bool ShareFormula { get; set; }
        public bool CopyFormulaFromShareLink { get; set; }

        #region FormulaTask

        public bool FormulaUiEditor { get; set; }
        public bool FormulaUiEditorCreateTask { get; set; }
        public bool FormulaUiEditorEditTask { get; set; }
        public bool FormulaUiEditorDeleteTask { get; set; }
        public bool FormulaUiEditorAddTaskConnection { get; set; }
        public bool FormulaUiEditorRemoveTaskConnection { get; set; }

        public bool FormulaTaskEditorModal { get; set; }
        public bool FormulaTaskEditorModalViewNotEdit { get; set; }
        public bool FormulaTaskEditorModalSetStartDate { get; set; }
        public bool FormulaTaskEditorModalSetDueDate { get; set; }
        public bool FormulaTaskEditorModalSetDuration { get; set; }
        public bool FormulaTaskEditorModalSetRecurringOptions { get; set; }
        public bool FormulaTaskEditorModalSetMembers { get; set; }
        public bool FormulaTaskEditorModalProfiles { get; set; }
        public bool FormulaTaskEditorModalFiles { get; set; }
        public bool FormulaTaskEditorModalEditCondition { get; set; }
        public bool FormulaTaskEditorModalAddConditionOption { get; set; }
        public bool FormulaTaskEditorModalEditConditionOption { get; set; }
        public bool FormulaTaskEditorModalDeleteConditionOption { get; set; }
        public bool FormulaTaskEditorModalAssignConditionOptionToTask { get; set; }
        public bool FormulaTaskEditorModalCreateChecklist { get; set; }
        public bool FormulaTaskEditorModalEditChecklist { get; set; }
        public bool FormulaTaskEditorModalDeleteChecklistItem { get; set; }
        public bool FormulaTaskEditorModalToggleChecklistItem { get; set; }
        public bool FormulaTaskLockTraining { get; set; }

        #endregion

        #endregion

        #region Project

        public bool ProjectsPage { get; set; }
        public bool CreateProject { get; set; }
        public bool EditProject { get; set; }
        public bool DeleteProject { get; set; }
        public bool ViewKanbanBoard { get; set; }
        public bool AccessToProjectUiEditor { get; set; }

        public bool ProjectModalWindow { get; set; }
        public bool SetProjectOwner { get; set; }
        public bool SetProjectMembers { get; set; }
        public bool SetProjectClient { get; set; }
        public bool AddProjectFiles { get; set; }


        public bool ChangeProjectTaskStatus { get; set; }
        public bool ImportTasksFromFormula { get; set; }
        public bool CreateProjectTask { get; set; }
        public bool EditProjectTask { get; set; }
        public bool DeleteProjectTask { get; set; }
        public bool SelectOptionOnCompleteProjectTask { get; set; }
        public bool ChangeChecklistItemStatus { get; set; }
        public bool FilterTasksBySkill { get; set; }
        public bool FilterTasksByUser { get; set; }

        public bool ProjectUiEditorCreateTask { get; set; }
        public bool ProjectUiEditorEditTask { get; set; }
        public bool ProjectUiEditorDeleteTask { get; set; }
        public bool ProjectUiEditorAddTaskConnection { get; set; }
        public bool ProjectUiEditorRemoveTaskConnection { get; set; }

        #region Project Task

        public bool CanChangeProcessingUser { get; set; }
        public bool CanAssignVendor { get; set; }
        public bool CanStopOutsource { get; set; }
        public bool ProjectTaskEditorModal { get; set; }
        public bool ProjectTaskEditorModalViewNotEdit { get; set; }
        public bool ProjectTaskEditorModalSetStartDate { get; set; }
        public bool ProjectTaskEditorModalSetDueDate { get; set; }
        public bool ProjectTaskEditorModalSetDuration { get; set; }
        public bool ProjectTaskEditorModalSetRecurringOptions { get; set; }
        public bool ProjectTaskEditorModalSetMembers { get; set; }
        public bool ProjectTaskEditorModalProfiles { get; set; }
        public bool ProjectTaskEditorModalFiles { get; set; }
        public bool ProjectTaskEditorModalEditCondition { get; set; }
        public bool ProjectTaskEditorModalAddConditionOption { get; set; }
        public bool ProjectTaskEditorModalEditConditionOption { get; set; }
        public bool ProjectTaskEditorModalDeleteConditionOption { get; set; }
        public bool ProjectTaskEditorModalAssignConditionOptionToTask { get; set; }
        public bool ProjectTaskEditorModalCreateChecklist { get; set; }
        public bool ProjectTaskEditorModalEditChecklist { get; set; }
        public bool ProjectTaskEditorModalDeleteChecklistItem { get; set; }
        public bool ProjectTaskEditorModalToggleChecklistItem { get; set; }

        #endregion

        #endregion

        #region Users

        public bool UsersPage { get; set; }
        public bool CreateUser { get; set; }
        public bool EditUsersName { get; set; }
        public bool ChangeUsersRole { get; set; }
        public bool DeleteUser { get; set; }
        public bool LockUser { get; set; }
        public bool UnlockUser { get; set; }
        public bool EditUserNotificationSettings { get; set; }
        public bool EditRoleNotificationSettings { get; set; }

        #endregion

        #region Profile

        public bool ViewOwnerBusinessDetails { get; set; }
        public bool EditOwnerBusinessDetails { get; set; }
        public bool ViewMissionControl { get; set; }
        public bool ViewUpSkills { get; set; }

        #endregion

        #region Team
        public bool SkillsPage { get; set; }
        public bool TeamsPage { get; set; }
        public bool CreateTeam { get; set; }
        public bool ReadSkill { get; set; }
        public bool CreateSkill { get; set; }
        public bool EditSkill { get; set; }
        public bool DeleteSkill { get; set; }
        public bool AssignSkill { get; set; }
        public bool ReadUserSkill { get; set; }
        public bool AddUserToSkill { get; set; }
        #endregion

        #region Client
        public bool ClientsPage { get; set; }
        public bool ReadClient { get; set; }
        public bool CreateClient { get; set; }
        public bool EditClient { get; set; }
        public bool DeleteClient { get; set; }
        public bool AssignProjectToClient { get; set; }
        public bool BalancePage { get; set; }
        public bool CreditsPage { get; set; }
        #endregion

        #region  SuperAdmin
        public bool adminUsersPage { get; set; }
        #endregion

        #region  PaymentPage
        public bool Payment { get; set; }
        #endregion

    }
}
