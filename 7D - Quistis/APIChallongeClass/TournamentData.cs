using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIChallongeClass
{
    public class TournamentData
    { 
        public class NonEliminationTournamentData
        {
        }

        public class OptionalDisplayData
        {
            public string show_standings { get; set; }
            public bool show_announcements { get; set; }
        }

        public class Root
        {
            public Tournament tournament { get; set; }
        }

        public class Tournament
        {
            public int id { get; set; }
            public string name { get; set; }
            public string url { get; set; }
            public string description { get; set; }
            public string tournament_type { get; set; }
            public object started_at { get; set; }
            public object completed_at { get; set; }
            public bool require_score_agreement { get; set; }
            public bool notify_users_when_matches_open { get; set; }
            public DateTime created_at { get; set; }
            public DateTime updated_at { get; set; }
            public string state { get; set; }
            public bool open_signup { get; set; }
            public bool notify_users_when_the_tournament_ends { get; set; }
            public int progress_meter { get; set; }
            public bool quick_advance { get; set; }
            public bool hold_third_place_match { get; set; }
            public string pts_for_game_win { get; set; }
            public string pts_for_game_tie { get; set; }
            public string pts_for_match_win { get; set; }
            public string pts_for_match_tie { get; set; }
            public string pts_for_bye { get; set; }
            public int swiss_rounds { get; set; }
            public bool @private { get; set; }
            public string ranked_by { get; set; }
            public bool show_rounds { get; set; }
            public bool hide_forum { get; set; }
            public bool sequential_pairings { get; set; }
            public bool accept_attachments { get; set; }
            public string rr_pts_for_game_win { get; set; }
            public string rr_pts_for_game_tie { get; set; }
            public string rr_pts_for_match_win { get; set; }
            public string rr_pts_for_match_tie { get; set; }
            public bool created_by_api { get; set; }
            public bool credit_capped { get; set; }
            public object category { get; set; }
            public bool hide_seeds { get; set; }
            public int prediction_method { get; set; }
            public object predictions_opened_at { get; set; }
            public bool anonymous_voting { get; set; }
            public int max_predictions_per_user { get; set; }
            public object signup_cap { get; set; }
            public object game_id { get; set; }
            public int participants_count { get; set; }
            public bool group_stages_enabled { get; set; }
            public bool allow_participant_match_reporting { get; set; }
            public object teams { get; set; }
            public object check_in_duration { get; set; }
            public object start_at { get; set; }
            public object started_checking_in_at { get; set; }
            public object tie_breaks { get; set; }
            public object locked_at { get; set; }
            public object event_id { get; set; }
            public object public_predictions_before_start_time { get; set; }
            public object ranked { get; set; }
            public object grand_finals_modifier { get; set; }
            public object predict_the_losers_bracket { get; set; }
            public object spam { get; set; }
            public object ham { get; set; }
            public object rr_iterations { get; set; }
            public object tournament_registration_id { get; set; }
            public object donation_contest_enabled { get; set; }
            public object mandatory_donation { get; set; }
            public NonEliminationTournamentData non_elimination_tournament_data { get; set; }
            public object auto_assign_stations { get; set; }
            public object only_start_matches_with_stations { get; set; }
            public string registration_fee { get; set; }
            public string registration_type { get; set; }
            public bool split_participants { get; set; }
            public object allowed_regions { get; set; }
            public object show_participant_country { get; set; }
            public object program_id { get; set; }
            public object program_classification_ids_allowed { get; set; }
            public object team_size_range { get; set; }
            public object toxic { get; set; }
            public object use_new_style { get; set; }
            public OptionalDisplayData optional_display_data { get; set; }
            public object processing { get; set; }
            public object oauth_application_id { get; set; }
            public object hide_bracket_preview { get; set; }
            public object consolation_matches_target_rank { get; set; }
            public string description_source { get; set; }
            public object subdomain { get; set; }
            public string full_challonge_url { get; set; }
            public string live_image_url { get; set; }
            public object sign_up_url { get; set; }
            public bool review_before_finalizing { get; set; }
            public bool accepting_predictions { get; set; }
            public bool participants_locked { get; set; }
            public object game_name { get; set; }
            public bool participants_swappable { get; set; }
            public bool team_convertable { get; set; }
            public bool group_stages_were_started { get; set; }
        }
    }
}
