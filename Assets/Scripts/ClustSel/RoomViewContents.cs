using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ClustSelNamespace {
public class RoomViewContents : MonoBehaviour {
    // Constants
    static private readonly Vector2 BatteryIconSize = new Vector2(5,5);
    static private readonly Vector2 GemIconSize = new Vector2(12,12);
    static private readonly Vector2 SnackIconSize = new Vector2(16,16);
    // Components
    [SerializeField] private RectTransform rt_props=null;
    // References
    private RoomData myRD;
    private RoomView myRoomView;
    
    // Getters (Private)
    private ResourcesHandler rh { get { return ResourcesHandler.Instance; } }


    // ================================================================
    //  Initialize
    // ================================================================
    public void Initialize (RoomView _roomView) {
        myRoomView = _roomView;
        myRD = myRoomView.MyRoomData;
        
        if (myRD.HasPlayerBeenHere) {
            AddPropImages();
        }
    }
    private void AddPropImages() {
        int snackIndex = 0; // for determining which Snacks we gots!
        foreach (PropData propData in myRD.allPropDatas) {
            // -- Spikes --
            if (propData.GetType() == typeof(SpikesData)) {
                SpikesData spikesData = propData as SpikesData;
                Color color = Colors.Spikes(myRD.WorldIndex);// new Color(0.7f,0.1f,0f, 0.6f);
                Image newObj = AddImage("Spikes", rh.s_spikes, rt_props, spikesData.myRect.position, spikesData.myRect.size, color);
                newObj.transform.localEulerAngles = new Vector3(0, 0, spikesData.rotation);
                newObj.type = Image.Type.Tiled;
                newObj.transform.localScale = Vector3.one / 100f; // kinda hacky-ish.
                newObj.rectTransform.sizeDelta *= 100f;
                newObj.transform.SetAsFirstSibling(); // put spikes BEHIND everything else.
            }
            // -- Grounds --
            else if (propData.GetType() == typeof(GroundData)) {
                GroundData pd = propData as GroundData;
                Color color = new Color255(100,130,90).ToColor();//Ground.GetBodyColor(pd, myRD.WorldIndex);
                AddImage("Ground", rh.s_ground, rt_props, pd.myRect.position, pd.myRect.size, color);
            }
            // -- DispGrounds --
            else if (propData.GetType() == typeof(DispGroundData)) {
                DispGroundData pd = propData as DispGroundData;
                Color color = DispGround.GetBodyColor(pd);
                color = new Color(color.r,color.g,color.b, color.a*0.6f); // alpha it out a bit, to taste.
                AddImage("DispGround", rh.s_ground, rt_props, pd.myRect.position, pd.myRect.size, color);
            }
            // -- Batteries --
            else if (propData.GetType() == typeof(BatteryData)) {
                BatteryData pd = propData as BatteryData;
                AddImage("Battery",rh.s_battery, rt_props, pd.pos, BatteryIconSize, Color.white);
            }
            // -- Gems --
            else if (propData.GetType() == typeof(GemData)) {
                GemData pd = propData as GemData;
                Sprite sprite = rh.GetGemSprite(pd.type);
                AddImage("Gem",sprite, rt_props, pd.pos, GemIconSize, Color.white);
            }
            // -- Snacks --
            else if (propData.GetType() == typeof(SnackData)) {
                SnackData pd = propData as SnackData;
                Color color;
                bool didEatSnack = SaveStorage.GetBool(SaveKeys.DidEatSnack(myRD, snackIndex));
                if (didEatSnack) { color = new Color(0,0,0, 0.2f); }
                else { color = PlayerBody.GetBodyColorNeutral(PlayerTypeHelper.TypeFromString(pd.playerType)); }
                AddImage("Snack",rh.s_snack, rt_props, pd.pos, SnackIconSize, color);
                snackIndex ++;
            }
        }
    }

    private Image AddImage(string goName, Sprite sprite, RectTransform tf_parent, Vector2 pos, Vector2 size, Color color) {
        pos -= myRD.cameraBoundsData.myRect.center; // hack-y. Works around the rooms' local/global alignment mismatch.
        GameObject iconGO = new GameObject ();
        Image img = iconGO.AddComponent<Image> ();
        img.name = goName;
        img.sprite = sprite;
        GameUtils.ParentAndReset(img.gameObject, tf_parent);
        img.rectTransform.anchoredPosition = pos;
        GameUtils.SizeUIGraphic(img, size);
        img.color = color;
        return img;
    }



}
}


