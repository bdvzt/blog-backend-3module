using System;
using System.Collections.Generic;

namespace backend_3_module.Models.Address;

/// <summary>
/// Сведения классификатора адресообразующих элементов
/// </summary>
public partial class AsAddrObj
{
    /// <summary>
    /// Уникальный идентификатор записи. Ключевое поле
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Глобальный уникальный идентификатор адресного объекта типа INTEGER
    /// </summary>
    public long Objectid { get; set; }

    /// <summary>
    /// Глобальный уникальный идентификатор адресного объекта типа UUID
    /// </summary>
    public Guid Objectguid { get; set; }

    /// <summary>
    /// Наименование
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Краткое наименование типа объекта
    /// </summary>
    public string? Typename { get; set; }

    /// <summary>
    /// Уровень адресного объекта
    /// </summary>
    public string Level { get; set; } = null!;

    /// <summary>
    /// Статус действия над записью – причина появления записи
    /// </summary>
    public int? Opertypeid { get; set; }

    /// <summary>
    /// Статус актуальности адресного объекта ФИАС
    /// </summary>
    public int? Isactual { get; set; }

    /// <summary>
    /// Признак действующего адресного объекта
    /// </summary>
    public int? Isactive { get; set; }
}
