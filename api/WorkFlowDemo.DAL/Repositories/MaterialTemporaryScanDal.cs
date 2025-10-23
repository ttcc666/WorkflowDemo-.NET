using SqlSugar;
using WorkFlowDemo.DAL.Base;
using WorkFlowDemo.Models.Entities;

namespace WorkFlowDemo.DAL.Repositories
{
    public class MaterialTemporaryScanDal : BaseRepository<MaterialTemporaryScan>
    {
        public MaterialTemporaryScanDal(ISqlSugarClient db) : base(db)
        {
        }
    }
}