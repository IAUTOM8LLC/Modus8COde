using IAutoM8.Global.Enums;
using IAutoM8.Service.Skills.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace IAutoM8.Service.Teams.Dto
{
    public class TeamDto
    {
        public int Id { get; set; }
        public string TeamName { get; set; }
        public bool TeamIsGlobal { get; set; }
        public int? TeamStatus { get; set; }

        public int SkillId { get; set; }
        public string SkillName { get; set; }
        public bool? SkillIsGlobal { get; set; }
        public int? SkillStatus { get; set; }

        //public Guid OutsourcerId { get; set; }
        public string OutsourcerName { get; set; }

        public int FormulaID { get; set; }
        public string FormulaName { get; set; }
        public bool? FormulaIsGlobal { get; set; }
        public DateTime FormulaCreatedDate { get; set; }
        public DateTime? FormulaUpdatedDate { get; set; }
        public int? FormulaStatus { get; set; }

        public int TaskID { get; set; }
        public string TaskName { get; set; }
        public bool? TaskIsGlobal { get; set; }
    }

    public class TeamData
    {
        public int Id { get; set; }
        public string TeamName { get; set; }
        public bool? isGlobal { get; set; }
        public bool? IsAdmin { get; set; }
        public int? TeamStatus { get; set; }
        public List<SkillRef> Skill { get; set; }
    }

    public class SkillRef
    {
        public int SkillId { get; set; }
        public string SkillName { get; set; }
        public bool? SkillIsGlobal { get; set; }
        public int? SkillStatus { get; set; }
        public List<UsersRef> Users { get; set; }
        public List<FormulasTaskRef> Formulas { get; set; }
    }

    public class UsersRef
    {
        public Guid OutsourcerId { get; set; }
        public string OutsourcerName { get; set; }
    }

    public class FormulasTaskRef
    {
        public int Id { get; set; }
        public string FormulaName { get; set; }
        public DateTime FormulaCreatedDate { get; set; }
        public DateTime? FormulaUpdatedDate { get; set; }
        public bool? FormulaIsGlobal { get; set; }
        public int? FormulaStatus { get; set; }
        public int TaskID { get; set; }
        public string TaskName { get; set; }
        public bool? TaskType { get; set; }
        public string OutsourcerName { get; set; }
        public bool ShowFormulaPublish { get; set; }
    }

    public class PublishStatus
    {
        public int Result { get; set; }
    }

    public class CopyFormula
    {
        public int OldFormulaId { get; set; }
        public int NewFormulaID { get; set; }
        public int OldTaskId { get; set; }
        public int NewTaskId { get; set; }
    }

    public class PublishFormula
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public int NewFormulaId { get; set; }
        public int NewFormulaTaskId { get; set; }
        public int OldFormulaTaskId { get; set; }
    }

    public class PublishFormulaList
    {
        public string Formula { get; set; }
        public string TeamList { get; set; }
        public string SkillList { get; set; }
    }

    public class PublishNotification
    {
        public string Team { get; set; }
        public string TeamName { get; set; }
        public string TeamAddedUpdated { get; set; }
        public string Skill { get; set; }
        public string SkillName { get; set; }
        public string SkillAddedUpdated { get; set; }
        public string Formula { get; set; }
        public string FormulaName { get; set; }
        public string FormulaAddedUpdated { get; set; }
    }
}
