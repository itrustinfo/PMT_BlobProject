<%@ Page Title="" Language="C#" MasterPageFile="~/_master_pages/modal.Master" AutoEventWireup="true" CodeBehind="view-sitephotographs.aspx.cs" Inherits="ProjectManagementTool._modal_pages.view_sitephotographs" %>
<asp:Content ID="Content1" ContentPlaceHolderID="modal_master_head" runat="server">
     <script type="text/javascript">
        function DeleteItem() {
            if (confirm("Are you sure you want to delete ...?")) {
                return true;
            }
            return false;
    }
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="modal_master_body" runat="server">
    <form id="frmUploadSitePhotograph" runat="server">
        <div class="container-fluid" style="max-height:85vh; overflow-y:auto; min-height:80vh;">
            <div class="row">
                <div class="col-sm-12">
                    <div class="table-responsive">
                            <asp:DataList ID="GrdSitePhotograph" runat="server" DataKeyField="SitePhotoGraph_UID" RepeatColumns="3" HorizontalAlign="Center" CellPadding="10" RepeatDirection="Horizontal" OnDeleteCommand="GrdSitePhotograph_DeleteCommand">
                                <ItemTemplate>
                                    <div style="width:275px; float:left; border:1px solid Gray; text-align:center; background-color:#f2f2f2;">
                                        
                                        <div style="padding:10px;">
                                            <asp:Image ID="imgEmp" runat="server" Width="225px" ImageUrl='<%# Bind("Site_Image", "{0}")%>' /><br />
                                        <b><asp:Label ID="LblDescription" runat="server" Text='<%#Eval("Description")%>'></asp:Label></b>
                                        </div>
                                        <asp:LinkButton ID="lnkdelete" runat="server" OnClientClick="return DeleteItem()" CausesValidation="false" CommandName="delete"><span title="Delete" class="fas fa-trash"></span></asp:LinkButton>
                                    </div>
                                    </ItemTemplate>
                            </asp:DataList>
                            <asp:Label ID="LblMessage" runat="server" class="lblCss"  Text="No Site Photograph/s Found.."></asp:Label>
                            </div>
                    </div>
                </div>
            </div>
        </form>
        
        
</asp:Content>
