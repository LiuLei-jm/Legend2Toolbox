using System.ComponentModel.DataAnnotations;

namespace Legend2Toolbox.Domain.Enums;

public enum Roles
{
    [Display(Name = "超级管理员")]
    SuperAdmin,
    [Display(Name = "会员")]
    Member,
    [Display(Name = "普通用户")]
    Guest
}