using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ConferenceDTO
{
	public class Speaker
	{

    public int Id { get; set; }

    /// <summary>
    /// The name of the speaker
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Name { get; set; }

    /// <summary>
    /// Biographical information about our speaker
    /// </summary>
    [StringLength(4000)]
    public string Bio { get; set; }

    /// <summary>
    /// Where you can learn more about our speaker
    /// </summary>
    [StringLength(1000)]
    public virtual string WebSite { get; set; }

  }
}
