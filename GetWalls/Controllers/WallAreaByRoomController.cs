using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

namespace WallAreaByRoom.GetWalls.Controllers
{
    /// <summary></summary>
    public class WallAreaByRoomController
    {
        /// <summary>Текущая открытая модель.</summary>
        public Document Doc { get; }

        /// <summary>Связанный файл.</summary>
        public Document DocLink { get; }
        
        /// <summary>Выбранная комната.</summary>
        public Room SelectedRoom { get; }
        /// <summary></summary>
        public Element PickedElement { get; }
        
        private readonly Level CurrentLevel;
        private FamilySymbol CurrentSymbol = default;

        /// <summary>Конструктор класса SkBoardsController.</summary>
        public WallAreaByRoomController(Room room, Element pickedElement, Document doc)
        {
            SelectedRoom = room;
            PickedElement = pickedElement;
            if (pickedElement is RevitLinkInstance)
            {
                DocLink = (PickedElement as RevitLinkInstance).GetLinkDocument();
            }
            
            Doc = doc;
            CurrentLevel = SetCurrentLevel();
        }

        /// <summary></summary>
        public void Run()
        {
            var boundarySegments = SelectedRoom.GetBoundarySegments(new SpatialElementBoundaryOptions
            {
                SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Finish
            });

            List<Wall> walls = new List<Wall>();
            foreach (IList<BoundarySegment> bSegments in boundarySegments)
            {
                foreach (BoundarySegment bSegment in bSegments)
                {
                    if (DocLink.GetElement(bSegment.ElementId) is HostObject host)
                    {
                        if (host is Wall wall)
                        {
                            walls.Add(wall);
                        }
                    }
                }
            }

            Debug.WriteLine(walls.Count);
            List<Wall> subWalls = new List<Wall>();
            Wall subWall;
            foreach (var wall in walls) // CGN_Стандарт_Тип     Нр-С
            {
                var wallLength = Math.Round(wall.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH).AsDouble() / 0.0032808d, 3);
                var wallArea = Math.Round(wall.get_Parameter(BuiltInParameter.HOST_AREA_COMPUTED).AsDouble() * 0.09290304d, 3);
                var wallVolume = Math.Round(wall.get_Parameter(BuiltInParameter.HOST_VOLUME_COMPUTED).AsDouble() / 35.315d, 3);

                Debug.WriteLine($"New wall Length: {wallLength} мм\tArea: {wallArea} м2\tVolume: {wallVolume} м3");
                
                //var compoundStruct = GetWallLayers(
                //    wall.Document.GetElement(
                //        wall.GetTypeId()));
                
                var bb = wall.get_BoundingBox(wall.Document.ActiveView);
                var elements = new FilteredElementCollector(DocLink)
                    .OfCategory(BuiltInCategory.OST_Walls)
                    .WherePasses(new BoundingBoxIntersectsFilter(new Outline(bb.Min, bb.Max), false))
                    .ToList();

                foreach (var e in elements)
                {
                    subWall = e as Wall;
                    
                    var subWallDir = ((subWall.Location as LocationCurve).Curve as Line).Direction;
                    var wallDir = ((wall.Location as LocationCurve).Curve as Line).Direction;
                    if (
                        IsEqualPoints(wallDir, subWallDir) ||
                        IsEqualPoints(wallDir.Negate(), subWallDir) ||
                        IsEqualPoints(wallDir, subWallDir.Negate())
                        )
                    { 
                        subWalls.Add(subWall);
                        Debug.WriteLine(subWall.Name);
                    }
                    
                }
                subWalls.Add(default);
            }

            Debug.WriteLine(subWalls.Count);
        }

        private CompoundStructureLayer[] GetWallLayers(Element element)
        {
            var attributes = element as HostObjAttributes;
            var compPoundStructure = attributes.GetCompoundStructure();
            var layers = compPoundStructure.GetLayers().OrderBy(x => x.LayerId).ToArray();

            //foreach (var layer in layers)
            //    Debug.WriteLine(
            //        $"{DocLink.GetElement(layer.MaterialId).Name}\t" +
            //        $"Thickness: {Math.Round(layer.Width * 304.8d, 2)}");
            
            return layers;
        }

        private bool IsEqualPoints(XYZ point1, XYZ point2) =>
            Math.Round(point1.X, 3) == Math.Round(point2.X, 3) &&
            Math.Round(point1.Y, 3) == Math.Round(point2.Y, 3) &&
            Math.Round(point1.Z, 3) == Math.Round(point2.Z, 3);

        private Level SetCurrentLevel() =>
             new FilteredElementCollector(Doc)
                .OfCategory(BuiltInCategory.OST_Levels)
                .WhereElementIsNotElementType()
                .ToElements()
                .Cast<Level>()
                .ToArray()
                .FirstOrDefault(x => Math.Round(x.Elevation, 3) == Math.Round(SelectedRoom.Level.Elevation, 3));
    }
}
